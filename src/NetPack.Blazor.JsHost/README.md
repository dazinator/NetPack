# NetPack.Blazor.JsHost

Blazor component library for hosting NetPack-managed JavaScript applications with explicit host capabilities integration.

## Overview

`NetPack.Blazor.JsHost` enables you to host JavaScript applications within Blazor WebAssembly while providing controlled access to Blazor services through a well-defined host API. This allows:

- **Fast development loop**: JS changes apply via HMR without restarting Blazor
- **Controlled integration**: JS can opt-in to using Blazor services (HttpClient, auth, config)
- **Portable code**: JS apps work both standalone and inside a Blazor host
- **No monkey patching**: Clean, explicit API instead of global overrides

## Features

### Host Capabilities

When running inside a Blazor host, JavaScript applications can access:

- **HTTP Client**: Use Blazor's HttpClient with auth headers, base address, and configured handlers
- **Authentication**: Get current user info and check permissions
- **Configuration**: Access application configuration values
- **Events**: Raise events that the host can handle

### Dual-Mode Operation

JavaScript applications work in two modes:

1. **Hosted Mode**: Running inside Blazor with access to host capabilities
2. **Standalone Mode**: Running independently using browser APIs as fallback

## Quick Start

### 1. Add to Your Blazor App

Install the package (when published):

```bash
dotnet add package NetPack.Blazor.JsHost
```

### 2. Configure Services

In your Blazor app's `Program.cs`:

```csharp
using NetPack.Blazor.JsHost;

builder.Services.AddNetPackJsHost();
```

### 3. Add the Script Reference

In your `index.html`:

```html
<script src="_content/NetPack.Blazor.JsHost/netpack-jshost.js"></script>
```

### 4. Use the Component

In your Razor page or component:

```razor
@using NetPack.Blazor.JsHost

<JsHostComponent AppUrl="/js/my-app.js" StartFunction="start" />
```

### 5. Create Your JavaScript App

Create a JavaScript module with a `start` function:

```javascript
// my-app.js
export function start(element, env) {
    // Check if running in hosted mode
    const isHosted = !!env.hostCapabilities;
    
    if (isHosted) {
        // Use Blazor's HttpClient
        const response = await env.hostCapabilities.fetch({
            url: '/api/data',
            method: 'GET'
        });
        
        // Get user info
        const user = await env.hostCapabilities.getUserInfo();
        console.log('User:', user.userName);
    } else {
        // Use browser fetch
        const response = await env.fallbackTransport.fetch({
            url: '/api/data',
            method: 'GET'
        });
    }
    
    // Render your app
    element.innerHTML = '<h1>Hello from JS!</h1>';
    
    // Optionally return cleanup function
    return {
        dispose() {
            element.innerHTML = '';
        }
    };
}
```

## TypeScript Support

TypeScript definitions are included. Use them for type safety:

```typescript
import { HostEnv, JsApp } from './netpack-jshost';

export function start(element: HTMLElement, env: HostEnv): JsApp {
    // Your code with full IntelliSense
}
```

## API Reference

### Component Parameters

- `AppUrl` (required): URL to the JavaScript module entry point
- `StartFunction`: Name of the function to call (default: "start")
- `ElementId`: Custom element ID (auto-generated if not provided)
- `HostCapabilities`: Custom host capabilities implementation
- `OnHostEvent`: Event handler for events raised from JS

### Host Capabilities Interface

```csharp
public interface IHostCapabilities
{
    Task<bool> CheckPermissionAsync(string permission);
    Task<UserInfo> GetUserInfoAsync();
    Task<string> GetConfigAsync(string key);
    Task RaiseHostEventAsync(string eventName, object eventData);
    Task<HttpResponse> FetchAsync(HttpRequest request);
}
```

### JavaScript HostEnv

```typescript
interface HostEnv {
    hostCapabilities?: HostCapabilities;  // Present when hosted
    fallbackTransport: FallbackTransport; // Always available
}
```

## Integration with NetPack Pipelines

Use NetPack pipelines to build and bundle your JS apps with HMR:

```csharp
services.AddNetPack((setup) =>
{
    setup.AddPipeline(pipelineBuilder =>
    {
        pipelineBuilder.WithHostingEnvironmentWebrootProvider()
            .AddRollupPipe(input =>
            {
                input.Include("js-src/**/*.js");
            }, options =>
            {
                options.Input = "js-src/main.js";
                options.Output = "js/my-app.js";
                options.Format = "esm";
            })
            .Watch();
    });
});
```

## Advanced Usage

### Custom Host Capabilities

Implement `IHostCapabilities` for custom behavior:

```csharp
public class CustomHostCapabilities : IHostCapabilities
{
    public async Task<bool> CheckPermissionAsync(string permission)
    {
        // Your custom permission logic
    }
    // ... implement other methods
}

// Register it
builder.Services.AddNetPackJsHost<CustomHostCapabilities>();
```

### Handling Host Events

```razor
<JsHostComponent 
    AppUrl="/js/my-app.js" 
    OnHostEvent="HandleHostEvent" />

@code {
    private async Task HandleHostEvent(HostEventArgs args)
    {
        Console.WriteLine($"Event: {args.EventName}");
        // Handle the event
    }
}
```

## License

This project is part of NetPack and uses the same license.
