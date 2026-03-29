(function() {
    'use strict';
    
    const SESSION_EXPIRED_RELOAD_DELAY_MS = 2000;
    
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

        if (!modal || !messageEl || !retryBtn) {
            return;
        }

        retryBtn.addEventListener('click', function() {
            messageEl.textContent = 'Reconnecting...';
            retryBtn.style.display = 'none';
            modal.classList.remove('components-reconnect-failed');
            
            Blazor.reconnect().catch(function() {
                setTimeout(function() {
                    modal.classList.add('components-reconnect-failed');
                    messageEl.textContent = 'Connection failed. Click Retry to try again.';
                    retryBtn.style.display = 'inline-block';
                }, 1000);
            });
        });

        modal.addEventListener('components-reconnect-state-changed', function(event) {
            var state = event.detail.state;
            var stateClasses = ['components-reconnect-show', 'components-reconnect-hide', 'components-reconnect-failed', 'components-reconnect-rejected'];
            modal.classList.remove.apply(modal.classList, stateClasses);

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

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initReconnectModal);
    } else {
        initReconnectModal();
    }
})();
