import type {
    Config,
    PagedResultCommandStatus,
    Resource,
    Setup
} from "$lib/Models";
import { tick } from "svelte"
import {get, type Readable, writable, type Writable} from "svelte/store";
import {SagaFlowSignalRHubClient} from "./SagaFlowSingalRHubClient";

export interface ISagaFlowServerState {
    setup: Setup;
    
    hasErrorFetchingConfig: boolean,
    fetchConfigError?: Error,
    
    config: Config;
    resourceCache: Record<string, Resource[]>;

    hasCommandStatuses: boolean;
    commandStatuses: PagedResultCommandStatus;
    
    signalRHub?: SagaFlowSignalRHubClient
}

// Using a convenient placeholder text that can be used to replace the sagaFlow route defined
// in the web application when initialized with a custom route, default route is sagaflow:
// builder.Services.AddSagaFlow(
//    options => {},
//    apiBasePath: "my-custom-route" )  
const sagaFlowDefaultRoute: string = "__default_saga_flow_route_placeholder__";

const initialState: ISagaFlowServerState = {
    setup: {
        baseUrl: "", // Blank refers to the local SagaFlow instance on a Web Application
        apiRoot: sagaFlowDefaultRoute,
        webRoot: sagaFlowDefaultRoute
    },
    hasErrorFetchingConfig: false,
    config: {
        commands: {},
        resourceLists: {}
    },
    
    resourceCache: {},

    hasCommandStatuses: false,
    commandStatuses: {
        page: [],
        keyword: '',
        pageIndex: 0,
        pageSize: 20,
        total: 0
    }
}

export const defaultSagaFlowServer: string = "__default_saga_flow_server__";

interface SagaFlowInitializationOptions {
    baseUrl?: string,
    apiRoot?: string,
    webRoot?: string
}

const defaultSagaFlowInitializationOptions: SagaFlowInitializationOptions = {
    baseUrl: "",
    apiRoot: sagaFlowDefaultRoute,
    webRoot: sagaFlowDefaultRoute
}

class SagaFlow
{
    // a dictionary of configured SagaFlow servers, there will always be one server available the default Sagaflow server for
    // most use cases where we are using the web-components to talk the SagaFlow instance attached to the host web application
    // using SagaFlow.
    private readonly _servers: Record<string, Writable<ISagaFlowServerState>>;
    
    constructor() {
        this._servers = {
            [defaultSagaFlowServer]: writable(initialState)
        }
    }
    
    public state(serverKey: string = defaultSagaFlowServer): Readable<ISagaFlowServerState> {
        return this.getServerStore(serverKey);
    }
    
    public currentValues(serverKey: string = defaultSagaFlowServer) : ISagaFlowServerState {
        return get(this.getServerStore(serverKey));
    }
    
    // The initialization call used to initialize the SagaFlow web-components with routes other than the default 
    // sagaflow routes.
    public initialize(
        options: SagaFlowInitializationOptions = {},
        serverKey: string = defaultSagaFlowServer)
    {
        // Make sure a store existing for the Server Key
        this.ensureServerStoreExists(serverKey);
        
        options = {
            // Apply default options
            ... defaultSagaFlowInitializationOptions, 
            
            // override with any options values supplied
            ... options 
        };
        
        console.debug("SagaFlow.initialize: started");

        const baseUrl = options.baseUrl.replace(/\/$/, "");
        const webRoot = options.webRoot.replace(/\/$/, "");
        const apiRoot = options.apiRoot.replace(/\/$/, "");
        
        const setup: Setup = {
            baseUrl,
            webRoot,
            apiRoot,
        }

        console.debug(`SagaFlow.initialize:     using apiUrl: ${setup.baseUrl} setup webRoot: ${setup.webRoot}, apiRoot: ${setup.apiRoot}`);
        fetch(`${baseUrl}/${apiRoot}/schema`)
            .then(async response => await this.processInitializationResponse(setup, response, serverKey))
            .catch(error => this.handleInitializationErrors(error, setup, serverKey))
            .finally(() => console.debug("SagaFlow.initialize: completed"));
    }
    
    // Returns a list of available resources for the provided resource id.
    public async getResources(resourceId: string, serverKey: string = defaultSagaFlowServer): Promise<Resource[]> {
        const store = this.getServerStore(serverKey)
        const { setup, config, resourceCache } = get(store);

        const resource = config.resourceLists[resourceId];
        
        if (!resource) throw Error(`Resource ${resourceId} does not exist`);

        if (resourceCache[resourceId]) return resourceCache[resourceId];

        const response = await fetch(`${setup.baseUrl}/${resource.href}`);

        if (!response.ok) throw Error(`Unable to fetch list of ${resourceId}`)

        if (response.ok) {
            const data: Resource[] = await response.json();

            store.update(s => ({
                ...s,
                resourceCache: {
                    ...resourceCache,
                    [resourceId]: data
                }
            }))

            return data;
        }
    }
    
    // Sends a command to the SagaFlow webapi to execute a SagaFlow command or job.
    public async sendCommandAsync<TCommand>(commandId: string, command: TCommand, serverKey: string = defaultSagaFlowServer): Promise<void> {
        const { setup, config } = get(this.getServerStore(serverKey));
        const commandDefinition = config.commands[commandId];
        
        if (!commandDefinition)

        console.debug("SagaFlow.sendCommandAsync: started");

        console.debug("SagaFlow.sendCommandAsync:   sending command:", command);
        console.debug(`SagaFlow.sendCommandAsync:   to ${setup.baseUrl}/${commandDefinition.href}`);
        
       try {
           const response= await fetch(`${setup.baseUrl}/${commandDefinition.href}`, {
               method: "POST",
               headers: {
                   'Accept': 'application/json',
                   'Content-Type': 'application/json'
               },
               body: JSON.stringify(command),
           });
       }
       catch (error) {
           console.error("SagaFlow.sendCommandAsync:    Error encountered: ", error);
       }
       finally {
           console.debug("SagaFlow.sendCommandAsync: completed"); 
       }
    }
    
    public async getStatuses(pageIndex: number, pageSize: number, keyword: string, serverKey: string = defaultSagaFlowServer) {
        const store = this.getServerStore(serverKey)
        const { setup} = get(store);
        
        console.debug("SagaFlow.getStatuses: started");

        console.debug(`SagaFlow.getStatuses:   sending command: pageIndex: ${pageIndex}, pageSize: ${pageSize}, keyworkd: ${keyword}`);

        const response = await fetch(`${setup.baseUrl}/${setup.apiRoot}/commands?pageIndex=${pageIndex}&pageSize=${pageSize}&keyword=${encodeURIComponent(keyword)}`);

        if (!response.ok) throw Error(`Unable to fetch statuses`);

        if (response.ok) {
            const data: PagedResultCommandStatus = await response.json();

            store.update(s => ({
                ...s,
                hasCommandStatuses: true,
                commandStatuses: {
                    ... data,
                    keyword,
                    page: data.page.map(item => ({ 
                        ... item,
                        startDateTime: new Date(item.startDateTime), 
                        finishDateTime: item.finishDateTime && new Date(item.finishDateTime)
                    }))
                }
            }))

            return data;
        }
    }
    
    private async processInitializationResponse(setup: Setup, response: Response, serverKey: string)
    {
        if (!response.ok)
            throw await response.json();
        
        const config: Config = await response.json();
        console.debug(`SagaFlow.initialize:     resolved configuration: `, config);
        
        const store = this.getServerStore(serverKey);

        store.update(s => ({
            ... s,
            setup,
            hasErrorFetchingConfig: false,
            fetchConfigError: undefined,
            config
        }));
        
        await tick();

        const signalRHub = new SagaFlowSignalRHubClient(serverKey, store);
        await signalRHub.start();

        store.update(s => ({
            ... s,
            signalRHub
        }))
    }

    private handleInitializationErrors(error: Error, setup: Setup, serverKey: string) {
        
        const store = this.getServerStore(serverKey);
        
        store.update(s => ({
            ... s,
            setup,
            hasErrorFetchingConfig: true,
            fetchConfigError: error
        }))
    }
    
    private getServerStore(serverKey: string): Writable<ISagaFlowServerState>
    {
        return this._servers[serverKey];
    }

    private ensureServerStoreExists(serverKey: string): Writable<ISagaFlowServerState>
    {
        let store = this._servers[serverKey];

        if (!store)
            store = this._servers[serverKey] = writable(initialState);
        
        return store;
    }
}

const sagaFlow: SagaFlow = new SagaFlow();
sagaFlow.initialize();

declare global {
    interface Window { SagaFlow: SagaFlow }
}

window.SagaFlow ??= sagaFlow;

export default sagaFlow;