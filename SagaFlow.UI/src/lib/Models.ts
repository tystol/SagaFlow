export interface Setup {
    // The base url for the SagaFlow instance, when using the UI against the local SagaFlow instance then this value is blank, when using the UI against a SagaFlow instance  
    baseUrl: string;
    apiRoot: string;
    webRoot: string;
};

export interface Resource {
    id: string;
    name: string;
};

export interface PaginatedResult<T> {
    items: T[];
    page: number;
    pageSize: number;
    totalItems?: number;
    totalPages?: number;
}

export interface ResourceList {
    name: string;
    href: string;
    schema: Record<string, Parameter>;
};

export interface Parameter {
    name: string;
    description: string;
    type: string;
    required: boolean;
    resourceListId: string;
};

export interface Command {
    name: string;
    description: string;
    href: string;
    schema: Record<string, Parameter>;
};

export interface Config {
    resourceLists: Record<string, ResourceList>;
    commands: Record<string, Command>;
};

export enum Status {
    Started = "Started",
    Processing = "Processing",
    Completed = "Completed",
    Errored = "Errored"
}

export interface CommandHistory {
    commandExecutionId: string;
    status: Status;
    name: string;
    commandName: string;
    commandType: string;
    
    initiatingUser: string;
    startDateTime: Date;
    finishDateTime?: Date;
    
    progress: number;

    humanReadableCommandPropertyValues: { [key:string]: string };
    
    lastError?: string;
    stackTrace?: string;
}