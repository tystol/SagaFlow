<svelte:options customElement="sf-command-form" />
<script lang="ts">
    import type {Command, Config} from "$lib/types";
    import sagaFlow, {defaultSagaFlowServer} from "../state/SagaFlowState";
    import {onDestroy} from "svelte";
    import ResourceSelector from "$lib/ResourceSelector.svelte";

    export let serverkey: string = defaultSagaFlowServer;
    export let commandid: string;

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
            await sagaFlow.sendCommandAsync(commandid, command, serverkey);
        }
        catch (error)
        {
            showError(error);
        }
    }

    const resetCommand = () => sendingCommand = false;
    
    const showError = <TError>(error: TError) => {
        console.error("Error: ", error);
        
        lastErrorMessage = `${error}`;
    }
    
    let store = sagaFlow.state(serverkey);
    
    $: config = $store.config;
    $: commandDefinition = config.commands[commandid];
    
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
                <span class="name">{parameterId}</span>
                <span class="value">
                {#if parameter.type === "String"}
                    <input type="text" name={parameterId} required={parameter.required} disabled={sendingCommand} />
                {:else if parameter.type === "Boolean"}
                    <label><input type="radio" name={parameterId} value={true} disabled={sendingCommand}>Yes</label>
                    <label><input type="radio" name={parameterId} value={false} disabled={sendingCommand}>No</label>
                {:else if resourceList}
                    <ResourceSelector serverKey={serverkey} resourceId={parameter.resourceListId} {parameterId} {parameter} disabled={sendingCommand} />
                {/if}
                </span>
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