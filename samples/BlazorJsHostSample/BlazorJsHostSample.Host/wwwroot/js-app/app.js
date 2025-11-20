/**
 * Sample NetPack JavaScript Application
 * Demonstrates host-aware JS integration with Blazor
 */

/**
 * Main entry point for the application
 * @param {HTMLElement} element - The container element
 * @param {import('../../_content/NetPack.Blazor.JsHost/netpack-jshost').HostEnv} env - Host environment
 */
export async function start(element, env) {
    console.log('Starting sample JS app...');
    console.log('Running in hosted mode:', !!env.hostCapabilities);

    // Create the UI
    const app = createApp(element, env);

    // Initialize based on mode
    if (env.hostCapabilities) {
        await initializeHostedMode(app, env);
    } else {
        await initializeStandaloneMode(app, env);
    }

    // Return app instance with dispose method
    return app;
}

function createApp(element, env) {
    const container = document.createElement('div');
    container.className = 'js-app-container';
    container.style.cssText = 'padding: 20px; border: 2px solid #0078d4; border-radius: 8px; margin: 20px 0; background: #f5f5f5;';

    const header = document.createElement('h2');
    header.textContent = 'NetPack JS Application';
    header.style.cssText = 'color: #0078d4; margin-top: 0;';
    
    const modeIndicator = document.createElement('div');
    modeIndicator.className = 'mode-indicator';
    modeIndicator.style.cssText = 'margin: 10px 0; padding: 10px; border-radius: 4px;';
    
    if (env.hostCapabilities) {
        modeIndicator.style.background = '#d4edda';
        modeIndicator.style.color = '#155724';
        modeIndicator.innerHTML = '<strong>🔗 Hosted Mode:</strong> Running inside Blazor with host capabilities';
    } else {
        modeIndicator.style.background = '#fff3cd';
        modeIndicator.style.color = '#856404';
        modeIndicator.innerHTML = '<strong>🌐 Standalone Mode:</strong> Running independently using browser APIs';
    }

    const content = document.createElement('div');
    content.className = 'app-content';
    content.style.cssText = 'margin-top: 20px;';

    const userInfo = document.createElement('div');
    userInfo.className = 'user-info';
    userInfo.style.cssText = 'margin: 10px 0; padding: 10px; background: white; border-radius: 4px;';

    const actions = document.createElement('div');
    actions.className = 'actions';
    actions.style.cssText = 'margin-top: 20px;';

    content.appendChild(userInfo);
    content.appendChild(actions);

    container.appendChild(header);
    container.appendChild(modeIndicator);
    container.appendChild(content);

    element.appendChild(container);

    return {
        container,
        userInfo,
        actions,
        dispose() {
            element.removeChild(container);
        }
    };
}

async function initializeHostedMode(app, env) {
    console.log('Initializing in hosted mode...');

    // Get user information from host
    try {
        const userInfo = await env.hostCapabilities.getUserInfo();
        
        app.userInfo.innerHTML = `
            <h3 style="margin-top: 0;">User Information (from Host)</h3>
            <p><strong>Authenticated:</strong> ${userInfo.isAuthenticated ? 'Yes ✓' : 'No ✗'}</p>
            <p><strong>Username:</strong> ${userInfo.userName || 'Anonymous'}</p>
            <p><strong>Email:</strong> ${userInfo.email || 'N/A'}</p>
        `;
    } catch (error) {
        app.userInfo.innerHTML = `<p style="color: red;">Error getting user info: ${error.message}</p>`;
    }

    // Add action buttons
    const fetchButton = createButton('Fetch Data (via Host)', async () => {
        try {
            const response = await env.hostCapabilities.fetch({
                url: 'https://api.github.com/repos/dazinator/NetPack',
                method: 'GET'
            });

            if (response.ok) {
                const data = JSON.parse(response.body);
                showNotification(app.actions, `Repository: ${data.full_name}, Stars: ${data.stargazers_count}`, 'success');
            } else {
                showNotification(app.actions, `HTTP ${response.status}: ${response.statusText}`, 'error');
            }
        } catch (error) {
            showNotification(app.actions, `Error: ${error.message}`, 'error');
        }
    });

    const eventButton = createButton('Raise Host Event', async () => {
        await env.hostCapabilities.raiseHostEvent('SampleEvent', {
            message: 'Hello from JavaScript!',
            timestamp: new Date().toISOString()
        });
        showNotification(app.actions, 'Event raised successfully!', 'success');
    });

    const configButton = createButton('Get Config', async () => {
        const value = await env.hostCapabilities.getConfig('SampleConfig');
        showNotification(app.actions, `Config value: ${value || 'Not set'}`, 'info');
    });

    app.actions.appendChild(fetchButton);
    app.actions.appendChild(eventButton);
    app.actions.appendChild(configButton);
}

async function initializeStandaloneMode(app, env) {
    console.log('Initializing in standalone mode...');

    app.userInfo.innerHTML = `
        <h3 style="margin-top: 0;">User Information</h3>
        <p><strong>Authenticated:</strong> No (standalone mode)</p>
        <p>Running without Blazor host capabilities.</p>
    `;

    const fetchButton = createButton('Fetch Data (via Browser)', async () => {
        try {
            const response = await env.fallbackTransport.fetch({
                url: 'https://api.github.com/repos/dazinator/NetPack',
                method: 'GET'
            });

            if (response.ok) {
                const data = JSON.parse(response.body);
                showNotification(app.actions, `Repository: ${data.full_name}, Stars: ${data.stargazers_count}`, 'success');
            } else {
                showNotification(app.actions, `HTTP ${response.status}: ${response.statusText}`, 'error');
            }
        } catch (error) {
            showNotification(app.actions, `Error: ${error.message}`, 'error');
        }
    });

    app.actions.appendChild(fetchButton);
}

function createButton(text, onClick) {
    const button = document.createElement('button');
    button.textContent = text;
    button.style.cssText = 'margin: 5px; padding: 10px 20px; background: #0078d4; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 14px;';
    button.onmouseover = () => button.style.background = '#005a9e';
    button.onmouseout = () => button.style.background = '#0078d4';
    button.onclick = onClick;
    return button;
}

function showNotification(container, message, type) {
    const notification = document.createElement('div');
    notification.style.cssText = 'margin: 10px 0; padding: 10px; border-radius: 4px; animation: fadeIn 0.3s;';
    
    const colors = {
        success: { bg: '#d4edda', color: '#155724' },
        error: { bg: '#f8d7da', color: '#721c24' },
        info: { bg: '#d1ecf1', color: '#0c5460' }
    };

    const style = colors[type] || colors.info;
    notification.style.background = style.bg;
    notification.style.color = style.color;
    notification.textContent = message;

    const existing = container.querySelector('.notification');
    if (existing) {
        existing.remove();
    }

    notification.className = 'notification';
    container.appendChild(notification);

    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}
