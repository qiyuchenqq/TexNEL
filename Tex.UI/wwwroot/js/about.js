function renderAbout(userData) {
    setTimeout(() => loadAboutInfo(), 0);
    return `
    <div class="about-page">
        <div class="about-hero">
            <img src="assets/Tex.png" alt="" class="about-logo" onerror="this.style.display='none'" />
            <div class="about-hero-text">
                <h1 class="about-title">Tex NEL</h1>
                <div class="about-version" id="about-version">...</div>
            </div>
        </div>
        <div class="about-card">
            <div class="about-section-title">版权信息</div>
            <p>Copyright &copy; 2026 祁宇晨66. All Rights Reserved.</p>
            <p class="about-muted">Thank Codexus!</p>
        </div>
        <div class="about-card">
            <div class="about-section-title">链接</div>
            <div class="about-links">
                <a class="about-link" href="#" id="about-link-qq">
                    <svg viewBox="0 0 16 16" fill="currentColor" width="16" height="16"><path d="M8 1C4.13 1 1 3.58 1 6.75c0 1.83 1.03 3.47 2.65 4.55-.07.52-.26 1.26-.7 1.95.92-.16 1.77-.56 2.4-1.05.51.12 1.06.18 1.65.18 3.87 0 7-2.58 7-5.63S11.87 1 8 1z"/></svg>
                    官方QQ群
                </a>
            </div>
        </div>
    </div>
    `;
}

async function loadAboutInfo() {
    const versionEl = document.getElementById('about-version');
    if (!versionEl) return;
    try {
        const r = await Bridge.send('system:about');
        if (r.success && r.data) {
            versionEl.textContent = 'Version ' + ('1.4.0' || '未知');
        }
    } catch {}

    document.getElementById('about-link-qq')?.addEventListener('click', (e) => {
        e.preventDefault();
        Bridge.send('system:openUrl', { url: 'https://qm.qq.com/q/BYKQ23CAJa' });
    });
}
