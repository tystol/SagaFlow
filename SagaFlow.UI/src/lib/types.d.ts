export type Setup = {
    // The base url for the SagaFlow instance, when using the UI against the local SagaFlow instance then this value is blank, when using the UI against a SagaFlow instance  
    baseUrl: string;
    apiRoot: string;
    webRoot: string;
};

export type Resource = {
    id: string;
    name: string;
};

export type ResourceList = {
    name: string;
    href: string;
};

export type Parameter = {
    name: string;
    description: string;
    type: string;
    required: boolean;
    resourceListId: string;
};

export type Command = {
    name: string;
    href: string;
    parameters: Record<string, Parameter>;
};

export type Config = {
    resourceLists: Record<string, ResourceList>;
    commands: Record<string, Command>;
};
