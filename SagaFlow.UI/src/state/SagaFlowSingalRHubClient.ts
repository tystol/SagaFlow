import * as signalR from "@microsoft/signalr";
import sagaFlow, {type ISagaFlowServerState} from "./SagaFlowState";
import {get, type Writable} from "svelte/store";
import type {SagaFlowCommandStatus} from "$lib/Models";
import debounce from "lodash/debounce";

export class SagaFlowSignalRHubClient {
    connection: signalR.HubConnection;
    debouncedGetStatuses = debounce(
        (pageIndex: number, pageSize: number, keyword: string) => sagaFlow.getStatuses(pageIndex, pageSize, keyword, this.serverKey),
        200);
    
    constructor(
        private serverKey: string,        
        private store: Writable<ISagaFlowServerState>) {
    }
    
    public async start() {
        const { setup } = get(this.store);

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${setup.baseUrl}/${setup.apiRoot}/update-hub`)
            .build();
        
        this.connection.on(
            "SendCommandStatusUpdate",
            (command: SagaFlowCommandStatus) => this.onSendCommandStatusUpdate(command)
        );

        this.connection.on(
            "SendCommandSucceeded",
            (command: SagaFlowCommandStatus) => this.onSendCommandStatusSucceeded(command)
        );

        this.connection.on(
            "SendCommandErrored",
            (command: SagaFlowCommandStatus) => this.onSendCommandStatusErrored(command)
        );
        
        try {

            await this.connection.start();
        }
        catch (err) {
            console.error("Error starting SagaFlow SignalR hub connection");
        }
        
    }
    
    private async onSendCommandStatusUpdate(command: SagaFlowCommandStatus) {
        const { commandStatuses } = get(this.store);
        
        const isUpdatedCommandOnPage= !!commandStatuses.page.find(item => item.sagaFlowCommandId === command.sagaFlowCommandId);
        if (isUpdatedCommandOnPage)
        {
            this.updateExistingCommandOnPage(command);
            
            return;
        }
        
        await this.debouncedGetStatuses(commandStatuses.pageIndex, commandStatuses.pageSize, commandStatuses.keyword);
    }

    private onSendCommandStatusSucceeded(command: SagaFlowCommandStatus) {
        document.dispatchEvent(new CustomEvent<SagaFlowCommandStatus>(
            "sf-command-succeeded",
            {
                detail: command
            }));
    }

    private onSendCommandStatusErrored(command: SagaFlowCommandStatus) {
        document.dispatchEvent(new CustomEvent<SagaFlowCommandStatus>(
            "sf-command-errored",
            {
                detail: command
            }));
    }
    
    private updateExistingCommandOnPage(command: SagaFlowCommandStatus) {
        this.store.update(s => ({
            ... s,
            commandStatuses: {
                ... s.commandStatuses,
                page: s.commandStatuses.page.map(item => {
                    if (item.sagaFlowCommandId === command.sagaFlowCommandId)
                        return { 
                            ... command,
                            
                            startDateTime: new Date(command.startDateTime),
                            finishDateTime: command.finishDateTime && new Date(command.finishDateTime)
                        };

                    return item;
                })
            }
        }))
    }
}