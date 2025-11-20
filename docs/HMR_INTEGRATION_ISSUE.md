# HMR Integration for Blazor JS Host

## Overview

Extend the Blazor JS Host component to support Hot Module Reload (HMR) for JavaScript applications using NetPack's existing Rollup pipeline infrastructure.

## Background

The Blazor JS Host feature enables hosting JavaScript applications within Blazor WebAssembly with explicit host capabilities. However, it currently requires manual browser refresh to see JavaScript changes. This issue tracks adding HMR support so JS changes apply automatically without restarting the Blazor app.

## Goals

1. **Fast Development Loop**: JS changes should apply instantly via HMR without Blazor restart
2. **Leverage Existing Infrastructure**: Reuse NetPack's existing Rollup pipeline and HMR mechanics
3. **Seamless Integration**: Work transparently with the JsHostComponent
4. **Configurable**: Allow developers to enable/disable HMR per environment

## Proposed Architecture

### Components Involved

1. **NetPack Rollup Pipeline** (existing)
   - Already supports HMR for regular web apps
   - Watches source files and rebuilds on changes
   - Serves updated modules

2. **JsHostComponent** (existing)
   - Currently loads JS modules via static imports
   - Needs HMR client integration

3. **HMR Client Bridge** (new)
   - Connects Blazor-hosted JS to NetPack HMR server
   - Listens for module update events
   - Triggers module reload and re-initialization

### High-Level Flow

```
JS Source Change
    ↓
NetPack Rollup Pipeline (watches, rebuilds)
    ↓
HMR Server emits update event
    ↓
HMR Client Bridge (in browser)
    ↓
JsHostComponent reloads module
    ↓
JS app re-initialized with preserved state (optional)
```

## Technical Requirements

### 1. Pipeline Configuration

Enable HMR in NetPack pipeline for Blazor projects:

```csharp
services.AddNetPack(setup =>
{
    setup.AddPipeline(builder =>
    {
        builder.WithHostingEnvironmentWebrootProvider()
            .AddRollupPipe(input =>
            {
                input.Include("js-src/**/*.js");
            }, options =>
            {
                options.Input = "js-src/main.js";
                options.Output = "js/bundle.js";
                options.Format = "esm";
                options.EnableHmr = true; // Enable HMR
            })
            .Watch();
    });
});
```

### 2. JsHostComponent HMR Support

Add HMR configuration to the component:

```razor
<JsHostComponent 
    AppUrl="/js-app/app.js" 
    EnableHmr="@isDevelopment"
    HmrServerUrl="@hmrServerUrl" />
```

### 3. HMR Client Bridge

Create a JavaScript module that:
- Connects to NetPack HMR server (WebSocket or polling)
- Listens for module update notifications
- Triggers module reload in JsHostComponent
- Optionally preserves application state

```javascript
// netpack-hmr-bridge.js
export class HmrBridge {
    constructor(serverUrl, onUpdate) {
        this.serverUrl = serverUrl;
        this.onUpdate = onUpdate;
    }
    
    connect() {
        // WebSocket connection to HMR server
    }
    
    handleUpdate(modulePath) {
        // Trigger reload callback
        this.onUpdate(modulePath);
    }
}
```

### 4. Module Reload Logic

Update JsHostComponent to:
- Detect when HMR is enabled
- Establish HMR connection
- Reload modules when updates are received
- Optionally call `dispose()` before reload
- Re-initialize app with new module

```csharp
private async Task HandleHmrUpdate(string modulePath)
{
    // Dispose current app instance
    if (_currentApp != null)
    {
        await JSRuntime.InvokeVoidAsync("NetPackJsHost.dispose", ElementId);
    }
    
    // Clear module cache (browser)
    await JSRuntime.InvokeVoidAsync("NetPackJsHost.clearModuleCache", modulePath);
    
    // Reload and restart
    await JSRuntime.InvokeVoidAsync("NetPackJsHost.loadAndStart", ElementId, AppUrl, StartFunction);
}
```

## Implementation Tasks

### Phase 1: Basic HMR Support
- [ ] Add HMR configuration options to JsHostComponent
- [ ] Create HMR client bridge JavaScript module
- [ ] Implement WebSocket/polling connection to NetPack HMR server
- [ ] Add module reload logic to JsHostComponent
- [ ] Test basic HMR functionality (reload on change)

### Phase 2: State Preservation
- [ ] Design state preservation API for JS apps
- [ ] Implement state save/restore in reload cycle
- [ ] Add configuration for state preservation strategy
- [ ] Document state preservation patterns

### Phase 3: Advanced Features
- [ ] Add HMR error handling and recovery
- [ ] Implement partial module updates (if supported)
- [ ] Add HMR connection status indicators
- [ ] Create development-only safety checks

### Phase 4: Documentation & Samples
- [ ] Update NetPack.Blazor.JsHost README with HMR setup
- [ ] Create HMR sample application
- [ ] Add troubleshooting guide
- [ ] Document HMR configuration options

## Sample Usage

After implementation, developers should be able to:

```csharp
// Program.cs
builder.Services.AddNetPack(setup =>
{
    setup.AddPipeline(builder =>
    {
        builder.WithHostingEnvironmentWebrootProvider()
            .AddRollupPipe(input => input.Include("js-src/**/*.js"), 
                options =>
                {
                    options.Input = "js-src/main.js";
                    options.Output = "js/bundle.js";
                    options.EnableHmr = true;
                })
            .Watch();
    });
});

builder.Services.AddNetPackJsHost();
```

```razor
@page "/app"
@inject IWebHostEnvironment Env

<JsHostComponent 
    AppUrl="/js/bundle.js"
    EnableHmr="@Env.IsDevelopment()" />
```

Then:
1. Edit JS source in `js-src/main.js`
2. NetPack Rollup rebuilds automatically
3. HMR pushes update to browser
4. JsHostComponent reloads module
5. Changes visible immediately without Blazor restart

## Benefits

1. **Faster Development**: No Blazor restart for JS changes
2. **Better DX**: Immediate feedback on JS modifications
3. **Existing Infrastructure**: Leverages NetPack's proven HMR system
4. **Optional**: Can disable in production, enable in development

## Challenges & Considerations

1. **Module Caching**: Browser may cache ES modules, need cache-busting
2. **State Loss**: Reloading loses JS app state unless preserved
3. **Connection Management**: HMR WebSocket needs reconnection logic
4. **Error Recovery**: Failed reloads should fall back gracefully
5. **Blazor Lifecycle**: Ensure HMR doesn't interfere with Blazor's own hot reload

## Related Work

- Existing NetPack HMR for SystemJS modules
- NetPack Browser Reload mechanics
- Existing Rollup pipeline infrastructure

## Acceptance Criteria

- [ ] JS source changes trigger automatic reload in JsHostComponent
- [ ] No Blazor app restart required for JS changes
- [ ] HMR can be enabled/disabled via configuration
- [ ] Works with existing NetPack Rollup pipelines
- [ ] Includes sample application demonstrating HMR
- [ ] Documented with setup instructions

## Implementation Notes

This builds on the foundation laid by the Blazor JS Host feature, which already provides:
- `JsHostComponent` for embedding JS apps
- `netpack-jshost.js` client integration
- Module loading infrastructure
- Dispose/cleanup lifecycle

The HMR integration will extend these capabilities to support hot reloading of JavaScript modules without losing the Blazor application state.

## References

- NetPack.Blazor.JsHost library documentation: `src/NetPack.Blazor.JsHost/README.md`
- Existing HMR implementation: `src/NetPack.HotModuleReload/`
- Rollup pipeline: `src/NetPack.Rollup/`
