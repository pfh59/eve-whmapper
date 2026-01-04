// Blazor JavaScript Initializers for .NET 9
// https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup#javascript-initializers

let isCustomEventRegistered = false;

/**
 * Called before Blazor Web App starts
 * @param {object} options - Blazor start options that can be modified
 */
export function beforeWebStart(options) {
    // Configure circuit options before start
    options.circuit ??= {};
    options.circuit.reconnectionOptions ??= {};
    options.circuit.reconnectionOptions.maxRetries = 8;
    options.circuit.reconnectionOptions.retryIntervalMilliseconds = getRetryInterval;
    
    options.circuit.configureSignalR = (builder) => {
        builder
            .withServerTimeout(60000)
            .withKeepAliveInterval(15000);
    };
}

/**
 * Called after Blazor Web App has started
 * @param {object} blazor - The Blazor instance
 */
export function afterWebStarted(blazor) {
    registerCustomEvents(blazor);
}

/**
 * Called before Blazor Server circuit starts
 * @param {object} options - Circuit options
 * @param {object} extensions - Extensions registry
 */
export function beforeServerStart(options, extensions) {
    // Server-specific initialization if needed
}

/**
 * Called after Blazor Server circuit is established
 * @param {object} blazor - The Blazor instance
 */
export function afterServerStarted(blazor) {
    registerCustomEvents(blazor);
}

/**
 * Calculate retry interval with exponential backoff
 * @param {number} previousAttempts - Number of previous retry attempts
 * @param {number} maxRetries - Maximum number of retries
 * @returns {number|null} - Interval in milliseconds or null to stop retrying
 */
function getRetryInterval(previousAttempts, maxRetries) {
    if (previousAttempts >= maxRetries) {
        return null;
    }
    // Exponential backoff: 1s, 2s, 4s, 8s, 15s, 30s, 30s, 30s
    const intervals = [1000, 2000, 4000, 8000, 15000, 30000, 30000, 30000];
    return intervals[Math.min(previousAttempts, intervals.length - 1)];
}

/**
 * Register custom Blazor events
 * @param {object} blazor - The Blazor instance
 */
function registerCustomEvents(blazor) {
    if (isCustomEventRegistered) {
        return;
    }

    try {
        blazor.registerCustomEventType('custompaste', {
            browserEventName: 'paste',
            createEventArgs: (event) => ({
                eventTimestamp: new Date().toISOString(),
                pastedData: event.clipboardData?.getData('text') ?? '',
            })
        });
        isCustomEventRegistered = true;
    } catch (error) {
        // Event already registered (can happen on reconnection)
        if (error.message?.includes('already registered')) {
            isCustomEventRegistered = true;
        } else {
            console.error('Failed to register custompaste event:', error);
        }
    }
}