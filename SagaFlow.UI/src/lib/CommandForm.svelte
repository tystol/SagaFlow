<svelte:options customElement="sf-command-form" />
<script lang="ts">
    import type {Command, Config} from "$lib/types";
    import sagaFlow, {defaultSagaFlowServer} from "../state/SagaFlowState";
    import ResourceSelector from "$lib/ResourceSelector.svelte";
    import {createEventDispatcher} from "svelte";

    export let serverKey: string = defaultSagaFlowServer;
    export let commandId: string;
    
    const dispatcher = createEventDispatcher();
    
    const wrappedDispatcher = <TDetail>(event: string, detail: TDetail) => {
        // Dispatch event for external event listeners
        document.dispatchEvent(new CustomEvent(
            event, 
            {
                detail,
                bubbles: true,
                composed: true,
            }))
        
        // Dispatch internal svelte events
        dispatcher(event, detail)
    }

    let commandDefinition: Command;
    let form: HTMLFormElement;
    let sendingCommand = false;
    
    let lastErrorMessage : string | undefined;

    let config: Config = {
        commands: {},
        resourceLists: {}
    };

    const sendCommand = async () => {
        if (sendingCommand) return;
        
        sendingCommand = true;

        const command = Object.fromEntries(new FormData(form).entries());
        
        try {
            await sagaFlow.sendCommandAsync(commandId, command, serverKey);

            wrappedDispatcher("sf-command-success", { serverKey, commandId, command });
        }
        catch (error)
        {
            showError(error);

            wrappedDispatcher("sf-command-error", { serverKey, commandId, command, error });
        }
        finally {
            wrappedDispatcher("sf-command-complete", { serverKey, commandId, command });
        }
    }

    const resetCommand = () => sendingCommand = false;
    
    const showError = <TError>(error: TError) => {
        console.error("Error: ", error);
        
        lastErrorMessage = `${error}`;
    }
    
    const resetFormOnCommandSelected = () => {
        sendingCommand = false;
        form?.reset();
    }
    
    let store = sagaFlow.state(serverKey);
    
    $: store = sagaFlow.state(serverKey);
    $: config = $store.config;
    $: commandDefinition = config.commands[commandId];
    
    $: commandId, resetFormOnCommandSelected();
    
</script>

<form class="command" bind:this={form} on:submit|preventDefault={sendCommand} on:reset={resetCommand}>
{#if commandDefinition}
    <div class="name">Command: {commandDefinition.name}</div>
    {#if lastErrorMessage}
        <div class="error">{lastErrorMessage}</div>
    {/if}
    <div>Parameters</div>
    <div class="parameters">
        {#each Object.entries(commandDefinition.parameters) as [parameterId, parameter]}
            {@const resourceList = parameter.resourceListId ? config.resourceLists[parameter.resourceListId] : null}
            <div class="parameter"
                class:required={parameter.required}>
                <label class="name" for={parameterId}>{parameter.name ?? parameterId}</label>
                <div class="value">
                {#if parameter.type === "String"}
                    <input type="text" id={parameterId} name={parameterId} required={parameter.required} disabled={sendingCommand} />
                {:else if parameter.type === "Boolean"}
                    <label><input type="radio" id={`${parameterId}_yes`} name={parameterId} value={true} disabled={sendingCommand}>Yes</label>
                    <label><input type="radio" id={`${parameterId}_no`} name={parameterId} value={false} disabled={sendingCommand}>No</label>
                {:else if resourceList}
                    <ResourceSelector serverKey={serverKey} resourceId={parameter.resourceListId} {parameterId} {parameter} disabled={sendingCommand} />
                {/if}
                </div>
                {#if parameter.description}
                    <div class="description">{parameter.description}</div>
                {/if}
            </div>
        {/each}
    </div>
{/if}
    <button type="submit" disabled={sendingCommand}>Run</button>
    <button type="reset">Reset</button>
</form>

<style lang="scss">
    .command {
      > .name {
        font-size: 1.125rem;
        font-weight: bold;
      }
      
      > .error {
        background: red;
        color: white;
        
        padding: 1rem 0.5rem;
      }
      
      .parameters {
        display: grid;
        grid-template-columns: minmax(6rem, auto) auto 1fr;
        grid-auto-rows: 1fr;
        gap: 0.5rem;

        > .parameter {
          display: contents;

          &.required {
            > .name::after {
              content: '*';
              color: red;
            }
          }

          > .name {
            grid-column: 1;
            justify-self: right;
            align-self: center;
          }

          > .value {
            grid-column: 2;
          }

          > .description {
            grid-column: 3;
            align-self: center;
          }
        }
      }
    }
</style>