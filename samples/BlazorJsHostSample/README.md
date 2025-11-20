# Blazor JS Host Sample

This sample demonstrates the NetPack Blazor JS Host integration, showing how to host JavaScript applications within Blazor WebAssembly with explicit host capabilities.

## What This Demonstrates

1. **Host-Aware JavaScript Application**: The JS app can detect whether it's running inside a Blazor host or standalone
2. **Host Capabilities**: JS can access Blazor services:
   - HTTP requests via Blazor's HttpClient
   - User authentication info
   - Configuration values
   - Raise events to the host
3. **Dual-Mode Operation**: The same JS app works standalone (using browser APIs) or hosted (using Blazor services)
4. **No Monkey Patching**: Clean, explicit API instead of overriding global functions

## Running the Sample

### Option 1: Run with Blazor Host

```bash
cd BlazorJsHostSample.Host
dotnet run
```

Then navigate to `/jshost-demo` to see the hosted JavaScript application.

### Option 2: Run JS App Standalone

Open `wwwroot/js-app/standalone.html` in a browser to see the same JavaScript app running without Blazor, using fallback browser APIs.

## Project Structure

- **BlazorJsHostSample.Host**: Blazor WebAssembly application that hosts the JS app
- **wwwroot/js-app/app.js**: The JavaScript application that works in both modes
- **Pages/JsHostDemo.razor**: Blazor page demonstrating the `JsHostComponent`

## Key Features

### JavaScript App (app.js)

The JS app exports a `start` function that receives:
- `element`: DOM element to render into
- `env`: Environment object with:
  - `hostCapabilities`: Available when running in Blazor (undefined in standalone)
  - `fallbackTransport`: Browser APIs for standalone mode

```javascript
export async function start(element, env) {
    if (env.hostCapabilities) {
        // Running in Blazor - use host capabilities
        const user = await env.hostCapabilities.getUserInfo();
    } else {
        // Running standalone - use browser APIs
        const response = await env.fallbackTransport.fetch({...});
    }
}
```

### Blazor Integration

The Blazor page uses the `JsHostComponent`:

```razor
<JsHostComponent 
    AppUrl="/js-app/app.js" 
    StartFunction="start"
    OnHostEvent="HandleHostEvent" />
```

## Try It Out

1. Click "Fetch Data (via Host)" to make an HTTP request using Blazor's HttpClient
2. Click "Raise Host Event" to send an event from JS to Blazor
3. Click "Get Config" to retrieve configuration from the host
4. Check the "User Information" section to see auth state from Blazor

The sample shows real-time interaction between JavaScript and Blazor without tight coupling.

## Extending This Sample

To add HMR support with NetPack pipelines:

1. Add NetPack services in the host app
2. Configure a Rollup pipeline for the JS app
3. Enable watch mode for auto-rebuild
4. Changes to JS will hot-reload without restarting Blazor

See the main NetPack.Blazor.JsHost README for more details.
