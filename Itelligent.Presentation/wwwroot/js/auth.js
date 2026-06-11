// On every page load, verify session with GET /api/auth/me.
// If no token or server rejects it, the navbar stays in "logged out" state.
$(function () {
    const token = localStorage.getItem('jwt_token');
    if (!token) return;

    $.ajax({
        url: '/api/auth/me',
        method: 'GET',
        headers: { 'Authorization': 'Bearer ' + token },
        success: function (user) {
            localStorage.setItem('jwt_user', JSON.stringify(user));
            document.cookie = 'jwt_token=' + token + '; path=/; max-age=3600';
            renderNavbar(user);
        },
        error: function (xhr) {
            // Token expirado o inválido → limpiar sesión completamente
            clearSession();
        }
    });
});

function renderNavbar(user) {
    $('#nav-login').hide();
    $('#nav-logout').show();
    $('#nav-username').show();
    $('#nav-user-display').text(user.username + ' (' + user.role + ')');
    $('#nav-manage').show();
}

function clearSession() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('jwt_user');
    document.cookie = 'jwt_token=; path=/; max-age=0';
}

function appLogout() {
    clearSession();
    window.location.href = '/auth/login';
}

function getToken() {
    return localStorage.getItem('jwt_token');
}

function authHeader() {
    const token = getToken();
    return token ? { 'Authorization': 'Bearer ' + token } : {};
}

function isLoggedIn() {
    return !!getToken();
}

function currentUser() {
    const json = localStorage.getItem('jwt_user');
    return json ? JSON.parse(json) : null;
}
