declare namespace signalR {
    class HubConnectionBuilder {
        withUrl(url: string, options: any): HubConnectionBuilder;
        build(): HubConnection;
    }

    class HubConnection {
        on(methodName: string, newMethod: (...args: any[]) => void): void;
        start(): Promise<void>;
        invoke(methodName: string, ...args: any[]): Promise<void>;
    }
}