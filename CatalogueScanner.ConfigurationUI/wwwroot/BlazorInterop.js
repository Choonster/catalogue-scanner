// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor.htm
// https://github.com/SamProf/MatBlazor/issues/663#issuecomment-763528584
function blazorGetTimezoneOffsetForDate(date) {
    return new Date(date).getTimezoneOffset();
}

function blazorClearAppServicesAuthenticationSession() {
    const url = new URL(window.location.href);
    url.pathname = '/ClearSession';
    url.searchParams.append('ReturnUrl', window.location.href);

    window.location.assign(url.toString());
}
