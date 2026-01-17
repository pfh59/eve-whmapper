// Blazor JavaScript Initializers for .NET 9
// https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup#javascript-initializers

let isCustomEventRegistered = false;
let countdownInterval = null;
const MAX_RETRIES = 8;

/**
 * Called before Blazor Web App starts
 * @param {object} options - Blazor start options that can be modified
 */
export function beforeWebStart(options) {
    // Configure circuit options before start
    options.circuit ??= {};
    options.circuit.reconnectionOptions ??= {};
    options.circuit.reconnectionOptions.maxRetries = MAX_RETRIES;
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
    const currentAttempt = previousAttempts + 1;
    updateReconnectUI(currentAttempt);
    
    if (previousAttempts >= maxRetries) {
        showReconnectFailed();
        return null;
    }
    
    // Exponential backoff: 1s, 2s, 4s, 8s, 15s, 30s, 30s, 30s
    const intervals = [1000, 2000, 4000, 8000, 15000, 30000, 30000, 30000];
    const interval = intervals[Math.min(previousAttempts, intervals.length - 1)];
    
    startCountdown(interval / 1000);
    
    return interval;
}

/**
 * Update the reconnection UI with current attempt info
 */
function updateReconnectUI(attempt) {
    const modal = document.getElementById('components-reconnect-modal');
    const attemptEl = document.getElementById('components-reconnect-current-attempt');
    const maxRetriesEl = document.getElementById('components-reconnect-max-retries');
    const messageEl = document.getElementById('reconnect-message');
    const retryBtn = document.getElementById('reconnect-retry-btn');
    
    if (modal) {
        modal.classList.remove('components-reconnect-hide', 'components-reconnect-failed');
        modal.classList.add('components-reconnect-show');
    }
    if (attemptEl) attemptEl.textContent = attempt;
    if (maxRetriesEl) maxRetriesEl.textContent = MAX_RETRIES;
    if (messageEl) messageEl.textContent = 'Attempting to reconnect...';
    if (retryBtn) retryBtn.style.display = 'none';
}

/**
 * Start countdown display
 */
function startCountdown(seconds) {
    const countdownEl = document.getElementById('components-seconds-to-next-attempt');
    if (!countdownEl) return;
    
    if (countdownInterval) {
        clearInterval(countdownInterval);
    }
    
    let remaining = Math.ceil(seconds);
    countdownEl.textContent = remaining;
    
    countdownInterval = setInterval(() => {
        remaining--;
        countdownEl.textContent = Math.max(0, remaining);
        if (remaining <= 0) {
            clearInterval(countdownInterval);
            countdownInterval = null;
        }
    }, 1000);
}

/**
 * Show failed state with retry button
 */
function showReconnectFailed() {
    const modal = document.getElementById('components-reconnect-modal');
    const messageEl = document.getElementById('reconnect-message');
    const retryBtn = document.getElementById('reconnect-retry-btn');
    const countdownEl = document.getElementById('components-seconds-to-next-attempt');
    const attemptEl = document.getElementById('components-reconnect-current-attempt');
    
    if (countdownInterval) {
        clearInterval(countdownInterval);
        countdownInterval = null;
    }
    
    // Apply failed state immediately
    applyFailedState();
    
    // Also apply after a delay to override any Blazor default behavior that might hide the modal
    setTimeout(applyFailedState, 100);
    setTimeout(applyFailedState, 500);
    setTimeout(applyFailedState, 1000);
    
    function applyFailedState() {
        if (modal) {
            modal.classList.remove('components-reconnect-hide');
            modal.classList.add('components-reconnect-show', 'components-reconnect-failed');
            modal.style.display = 'flex';
        }
        if (messageEl) messageEl.textContent = 'Connection failed. Click Retry to try again.';
        if (retryBtn) retryBtn.style.display = 'inline-block';
        if (countdownEl) countdownEl.textContent = '-';
        if (attemptEl) attemptEl.textContent = MAX_RETRIES;
    }
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