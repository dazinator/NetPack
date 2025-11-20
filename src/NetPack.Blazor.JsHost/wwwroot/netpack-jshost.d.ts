/**
 * NetPack Blazor JS Host TypeScript Definitions
 * Use these types when building JavaScript applications that integrate with Blazor hosts.
 */

/**
 * HTTP request to be executed by the host.
 */
export interface HostHttpRequest {
    url: string;
    method?: string;
    headers?: Record<string, string>;
    body?: string;
}

/**
 * HTTP response from the host.
 */
export interface HostHttpResponse {
    status: number;
    statusText: string;
    ok: boolean;
    headers: Record<string, string>;
    body: string;
}

/**
 * Information about the current user.
 */
export interface UserInfo {
    userName?: string;
    email?: string;
    isAuthenticated: boolean;
    claims: Record<string, string>;
}

/**
 * Host capabilities available when running inside a Blazor host.
 */
export interface HostCapabilities {
    /**
     * Check if the user has a specific permission.
     */
    checkPermission(permission: string): Promise<boolean>;

    /**
     * Get information about the current user.
     */
    getUserInfo(): Promise<UserInfo>;

    /**
     * Get a configuration value by key.
     */
    getConfig(key: string): Promise<string | null>;

    /**
     * Raise an event that the host can handle.
     */
    raiseHostEvent(eventName: string, eventData: any): Promise<void>;

    /**
     * Execute an HTTP request using the host's HttpClient.
     * This gets auth headers, base address, and other configured handlers.
     */
    fetch(request: HostHttpRequest): Promise<HostHttpResponse>;
}

/**
 * Fallback transport using browser APIs for standalone mode.
 */
export interface FallbackTransport {
    /**
     * Execute an HTTP request using the browser's fetch API.
     */
    fetch(request: HostHttpRequest): Promise<HostHttpResponse>;
}

/**
 * Environment provided to the JavaScript application at startup.
 */
export interface HostEnv {
    /**
     * Host capabilities - available when running inside a Blazor host.
     * Undefined when running standalone.
     */
    hostCapabilities?: HostCapabilities;

    /**
     * Fallback transport using browser APIs.
     * Always available.
     */
    fallbackTransport: FallbackTransport;
}

/**
 * Optional interface for JavaScript applications to implement.
 * The dispose method will be called when the component is unmounted.
 */
export interface JsApp {
    dispose?(): void;
}

/**
 * Standard entry point signature for NetPack JS applications.
 * @param element - The DOM element to render into
 * @param env - The host environment
 * @returns Optionally return an app instance with a dispose method
 */
export type AppStartFunction = (element: HTMLElement, env: HostEnv) => JsApp | Promise<JsApp> | void | Promise<void>;
