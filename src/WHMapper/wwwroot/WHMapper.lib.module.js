export function afterServerStarted(blazor) {
    setup(blazor);
}

export function afterWebAssemblyStarted(blazor) {
    setup(blazor);
}

function setup(blazor) {
    try {
        blazor.registerCustomEventType('custompaste', {
            browserEventName: 'paste',
            createEventArgs: event => ({
                eventTimestamp: new Date().toISOString(),
                pastedData: event.clipboardData.getData('text'),
            })
        });
    } catch (error) {
        // Event already registered, ignore
        if (!error.message?.includes('already registered')) {
            console.error('Failed to register custompaste event:', error);
        }
    }
}