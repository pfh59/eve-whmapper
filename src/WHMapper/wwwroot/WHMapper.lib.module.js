// Blazor JavaScript Initializers for .NET 9
// https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup#javascript-initializers

let isCustomEventRegistered = false;
let countdownInterval = null;
let reconnectionLoopRunning = false;
const MAX_RETRIES = 8;
const SESSION_EXPIRED_RELOAD_DELAY_MS = 3000;

const ReconnectState = Object.freeze({
    SHOW: 'show',
    HIDE: 'hide',
    RETRYING: 'retrying',
    FAILED: 'failed',
    REJECTED: 'rejected'
});

function initReconnectModal() {
    const modal = document.getElementById('components-reconnect-modal');
    const messageEl = document.getElementById('reconnect-message');
    const retryBtn = document.getElementById('reconnect-retry-btn');
    if (!modal || !messageEl || !retryBtn) return;

    retryBtn.addEventListener('click', function() {
        messageEl.textContent = 'Reconnecting...';
        retryBtn.style.display = 'none';
        retryBtn.hidden = true;
        modal.classList.remove('components-reconnect-failed');
        reconnectionLoopRunning = false;
        startReconnectionLoop();
    });

    modal.addEventListener('components-reconnect-state-changed', function(event) {
        const state = event.detail.state;
        const stateClasses = ['components-reconnect-show', 'components-reconnect-hide', 'components-reconnect-failed', 'components-reconnect-rejected'];
        modal.classList.remove(...stateClasses);
        switch (state) {
            case ReconnectState.SHOW:
                modal.classList.add('components-reconnect-show');
                messageEl.textContent = 'Attempting to reconnect...';
                retryBtn.hidden = true;
                break;
            case ReconnectState.HIDE:
                modal.classList.add('components-reconnect-hide');
                break;
            case ReconnectState.RETRYING:
                messageEl.textContent = 'Reconnecting...';
                break;
            case ReconnectState.FAILED:
                modal.classList.add('components-reconnect-failed');
                messageEl.textContent = 'Reconnection failed. Please try again.';
                retryBtn.hidden = false;
                break;
            case ReconnectState.REJECTED:
                modal.classList.add('components-reconnect-rejected');
                messageEl.textContent = 'Session expired. Reloading page...';
                retryBtn.hidden = true;
                setTimeout(function() { location.reload(); }, SESSION_EXPIRED_RELOAD_DELAY_MS);
                break;
        }
    });
}

// Initialisation au chargement du document pour garantir la gestion du modal même hors cycle Blazor
if (typeof window !== 'undefined') {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => initReconnectModal());
    } else {
        initReconnectModal();
    }
}

/**
 * Called before Blazor Web App starts
 * @param {object} options - Blazor start options that can be modified
 */
export function beforeWebStart(options) {
    options.circuit ??= {};

    // Custom reconnection handler — fires onConnectionDown immediately when the circuit drops
    options.circuit.reconnectionHandler = {
        onConnectionDown(opts, error) {
            console.warn('[WHMapper] Circuit connection down:', error?.message ?? 'unknown');
            showReconnectModal();
            startReconnectionLoop();
        },
        onConnectionUp() {
            console.info('[WHMapper] Circuit connection restored');
            reconnectionLoopRunning = false;
            hideReconnectModal();
        }
    };

    options.circuit.configureSignalR = (builder) => {
        builder
            .withServerTimeout(30000)
            .withKeepAliveInterval(10000);
    };
}

/**
 * Called after Blazor Web App has started
 * @param {object} blazor - The Blazor instance
 */
export function afterWebStarted(blazor) {
    registerCustomEvents(blazor);
    initReconnectModal();
    registerBrowserOfflineDetection();
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
    initReconnectModal();
    registerBrowserOfflineDetection();
}

/**
 * Register browser offline/online events as backup detection
 * (handles network-level disconnections that SignalR may not detect instantly)
 */
let browserOfflineRegistered = false;
function registerBrowserOfflineDetection() {
    if (browserOfflineRegistered) return;
    browserOfflineRegistered = true;

    window.addEventListener('offline', () => {
        console.warn('[WHMapper] Browser went offline');
        showReconnectModal();
    });

    window.addEventListener('online', () => {
        console.info('[WHMapper] Browser back online, triggering reconnect');
        if (window.Blazor && typeof window.Blazor.reconnect === 'function') {
            window.Blazor.reconnect();
        }
    });
}

/**
 * Show the reconnect modal immediately
 */
function showReconnectModal() {
    const modal = document.getElementById('components-reconnect-modal');
    const messageEl = document.getElementById('reconnect-message');
    const retryBtn = document.getElementById('reconnect-retry-btn');
    const attemptEl = document.getElementById('components-reconnect-current-attempt');
    const maxRetriesEl = document.getElementById('components-reconnect-max-retries');

    if (modal) {
        modal.classList.remove('components-reconnect-hide', 'components-reconnect-failed');
        modal.classList.add('components-reconnect-show');
        modal.style.display = 'flex';
    }
    if (messageEl) messageEl.textContent = 'Connection lost. Reconnecting...';
    if (retryBtn) {
        retryBtn.style.display = 'none';
        retryBtn.hidden = true;
    }
    if (attemptEl) attemptEl.textContent = '0';
    if (maxRetriesEl) maxRetriesEl.textContent = MAX_RETRIES;
}

/**
 * Hide the reconnect modal
 */
function hideReconnectModal() {
    const modal = document.getElementById('components-reconnect-modal');
    if (modal) {
        modal.classList.remove('components-reconnect-show', 'components-reconnect-failed');
        modal.classList.add('components-reconnect-hide');
        modal.style.display = '';
    }
    if (countdownInterval) {
        clearInterval(countdownInterval);
        countdownInterval = null;
    }
}

/**
 * Start the reconnection retry loop with exponential backoff
 */
function startReconnectionLoop() {
    if (reconnectionLoopRunning) return;
    reconnectionLoopRunning = true;

    const intervals = [1000, 2000, 4000, 8000, 15000, 30000, 30000, 30000];
    let attempt = 0;

    function tryReconnect() {
        if (!reconnectionLoopRunning) return;

        attempt++;
        updateReconnectAttempt(attempt);

        if (attempt > MAX_RETRIES) {
            showReconnectFailed();
            reconnectionLoopRunning = false;
            return;
        }

        const delay = intervals[Math.min(attempt - 1, intervals.length - 1)];
        startCountdown(delay / 1000);

        if (window.Blazor && typeof window.Blazor.reconnect === 'function') {
            window.Blazor.reconnect().then(function (success) {
                if (success) {
                    reconnectionLoopRunning = false;
                    hideReconnectModal();
                } else {
                    // Reconnect returned false — schedule next attempt
                    setTimeout(tryReconnect, delay);
                }
            }).catch(function () {
                setTimeout(tryReconnect, delay);
            });
        } else {
            setTimeout(tryReconnect, delay);
        }
    }

    // First attempt after a very short delay to allow UI to render
    setTimeout(tryReconnect, 500);
}

/**
 * Update attempt counter in the UI
 */
function updateReconnectAttempt(attempt) {
    const attemptEl = document.getElementById('components-reconnect-current-attempt');
    const messageEl = document.getElementById('reconnect-message');
    if (attemptEl) attemptEl.textContent = attempt;
    if (messageEl) messageEl.textContent = 'Attempting to reconnect...';
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
    
    if (modal) {
        modal.classList.remove('components-reconnect-hide');
        modal.classList.add('components-reconnect-show', 'components-reconnect-failed');
        modal.style.display = 'flex';
    }
    if (messageEl) messageEl.textContent = 'Connection failed. Click Retry to try again.';
    if (retryBtn) {
        retryBtn.style.display = 'inline-block';
        retryBtn.hidden = false;
    }
    if (countdownEl) countdownEl.textContent = '-';
    if (attemptEl) attemptEl.textContent = MAX_RETRIES;
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