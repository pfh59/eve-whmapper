export function afterServerStarted(blazor) {
    setup(blazor);
}

export function afterWebAssemblyStarted(blazor) {
    setup(blazor);
}

function setup(blazor) {
    blazor.registerCustomEventType('custompaste', {
        browserEventName: 'paste',
        createEventArgs: event => ({
            eventTimestamp: new Date().toISOString(),
            pastedData: event.clipboardData.getData('text'),
        })
    });
}