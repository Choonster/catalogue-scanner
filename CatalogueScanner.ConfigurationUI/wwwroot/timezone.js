export function getBrowserTimeZone() {
    const options = Intl.DateTimeFormat().resolvedOptions();

    console.log(options)

    return options.timeZone;
}
