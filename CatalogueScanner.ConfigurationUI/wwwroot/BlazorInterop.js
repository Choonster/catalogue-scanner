function blazorClearAppServicesAuthenticationSession() {
    const url = new URL(window.location.href);
    url.pathname = '/ClearSession';
    url.searchParams.append('ReturnUrl', window.location.href);

    window.location.assign(url.toString());
}
