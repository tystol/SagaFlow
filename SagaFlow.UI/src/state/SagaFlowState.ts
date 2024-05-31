import type {Config, Resource, Setup} from "$lib/types";
import {get, type Readable, writable, type Writable} from "svelte/store";

interface ISagaFlowServerState {
    setup: Setup;
    config: Config;
    resourceCache: Record<string, Resource[]>;
}

const initialState: ISagaFlowServerState = {
    setup: {
        apiRoot: "sagaflow",
        webRoot: "sagaflow"
    },
    config: {
        commands: {},
        resourceLists: {}
    },
    
    resourceCache: {}
}

export const defaultSagaFlowServer: string = "__default_saga_flow_server__";

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
    public initialize(baseUrl: string = "sagaflow", apiRoot: string | undefined = undefined, serverKey: string = defaultSagaFlowServer)
    {
        console.debug("SagaFlow.initialize: started");
        
        baseUrl = baseUrl.replace(/\/$/, "");

        apiRoot ??= baseUrl;
        apiRoot = apiRoot.replace(/\/$/, "");
        
        const setup: Setup = {
            webRoot: baseUrl,
            apiRoot,
        }

        console.debug(`SagaFlow.initialize:     using setup webRoot: ${setup.webRoot}, apiRoot: ${setup.apiRoot}`);
        fetch(`/${apiRoot}/schema`)
            .then(async response => await this.processInitializationResponse(setup, response, serverKey))
            .catch(error => Promise.reject(error))
            .finally(() => console.debug("SagaFlow.initialize: completed"));
    }
    
    // Returns a list of available resources for the provided resource id.
    public async getResources(resourceId: string, serverKey: string = defaultSagaFlowServer): Promise<Resource[]> {
        const store = this.getServerStore(serverKey)
        const { setup, config, resourceCache } = get(store);
        
        const resource = config.resourceLists[resourceId];
        
        if (!resource) throw Error(`Resource ${resourceId} does not exist`);
        
        if (resourceCache[resourceId]) return resourceCache[resourceId];

        const response = await fetch(`/${resource.href}`);

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
        const { config } = get(this.getServerStore(serverKey));
        const commandDefinition = config.commands[commandId];
        
        if (!commandDefinition)

        console.debug("SagaFlow.sendCommandAsync: started");

        console.debug("SagaFlow.sendCommandAsync:   sending command:", command);
        console.debug(`SagaFlow.sendCommandAsync:   to /${commandDefinition.href}`);
        
       try {
           const response= await fetch(`/${commandDefinition.href}`, {
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
    
    private async processInitializationResponse(setup: Setup, response: Response, serverKey: string)
    {
        if (!response.ok)
            Promise.reject(response);
        
        const config: Config = await response.json();
        console.debug(`SagaFlow.initialize:     resolved configuration: `, config);
        
        let store = this._servers[serverKey];
        
        if (!store)
            store = this._servers[serverKey] = writable(initialState);

        store.update(s => ({
            ... s,
            setup,
            config
        }))
    }
    
    private getServerStore(serverKey: string): Writable<ISagaFlowServerState>
    {
        const store = this._servers[serverKey]

        if (!store) throw Error(`Unknown server key: ${serverKey}`);

        return store;
    }
}

const sagaFlow: SagaFlow = new SagaFlow();
sagaFlow.initialize();

declare global {
    interface Window { SagaFlow: SagaFlow; }
}

window.SagaFlow ??= sagaFlow;

export default sagaFlow;