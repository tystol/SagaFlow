<svelte:options customElement={{
    tag: "sf-command-form",
    shadow: "none",
    extend: extendToGetWebComponentRoot
}} />
<script lang="ts">
    import type {Command, Config} from "$lib/types";
    import sagaFlow, {defaultSagaFlowServer} from "../state/SagaFlowState";
    import ResourceSelector from "$lib/ResourceSelector.svelte";
    import {
        createWebComponentEventDispatcher,
        extendToGetWebComponentRoot
    } from "$lib/RootWebComponentEventDispatcher";
    import StringParameter from "$lib/StringParameter.svelte";
    

    // When used as a web-component, this is required to dispatch events from the web component
    export let __webComponentElement: HTMLElement | undefined = undefined;
    
    export let serverKey: string = defaultSagaFlowServer;
    export let commandId: string;
    
    const dispatcher = createWebComponentEventDispatcher(__webComponentElement)

    let commandDefinition: Command;
    let form: HTMLFormElement;
    let sendingCommand = false;
    
    let lastErrorMessage : string | undefined;

    let config: Config = {
        commands: {},
        resourceLists: {}
    };

    // Public method to allow programmatically triggering the command form to submit from the web component instance
    export const submit = async () => {
        if (form.reportValidity()) await sendCommand();
    }

    // Public method to allow programmatically triggering the command form to be reset from the web component instance
    export const reset = () => {
        
        form.reset();
    }
    
    const  ensureBooleanValue = (fromEntries: any) => {

        Object.entries(commandDefinition.parameters).forEach(([key, parameter]) => {
           if (parameter.type !== "Boolean") return;

            fromEntries[key] = fromEntries[key] === "true";
        });
        
        return fromEntries;
    }
    
    const sendCommand = async () => {
        if (sendingCommand) return;
        
        sendingCommand = true;

        const command = ensureBooleanValue(Object.fromEntries(new FormData(form).entries()));
        
        try {
            await sagaFlow.sendCommandAsync(commandId, command, serverKey);

            dispatcher("sf-command-success", { serverKey, commandId, command });
        }
        catch (error)
        {
            showError(error);

            dispatcher("sf-command-error", { serverKey, commandId, command, error });
        }
        finally {
            dispatcher("sf-command-complete", { serverKey, commandId, command });
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

<form class="command-form" bind:this={form} on:submit|preventDefault={sendCommand} on:reset={resetCommand}>
{#if commandDefinition}
    <div class="name">{commandDefinition.name}</div>
    {#if lastErrorMessage}
        <div class="error">{lastErrorMessage}</div>
    {/if}
    {#if commandDefinition.description}
        <div class="description">{commandDefinition.description}</div>
    {/if}
    <div class="parameters">
        {#each Object.entries(commandDefinition.parameters) as [parameterId, parameter]}
            {@const resourceList = parameter.resourceListId ? config.resourceLists[parameter.resourceListId] : null}
            <div class="parameter"
                 class:required={parameter.required}
            >
                <label class="name" for={parameterId}>{parameter.name ?? parameterId}</label>
                
                <div class="value">
                {#if parameter.type === "String"}
                    <StringParameter {serverKey} {parameterId} {parameter} disabled={sendingCommand} />
                {:else if parameter.type === "Boolean"}
                    <label class="value-yes"><input type="radio" id={`${parameterId}_yes`} name={parameterId} required={parameter.required} value={true} disabled={sendingCommand}>Yes</label>
                    <label class="value-no"><input type="radio" id={`${parameterId}_no`} name={parameterId} required={parameter.required} value={false} disabled={sendingCommand}>No</label>
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
    <button class="btn-submit"
            type="submit" disabled={sendingCommand}>Run</button>
    <button class="btn-reset" 
            type="reset">Reset</button>
</form>

<style lang="scss">
    .command-form {
      > .name {
        font-size: var(--sf-command-form-name-font-size, 1.125rem);
        display: var(--sf-command-form-name-display, inherit);
        font-weight: var(--sf-command-form-name-font-weight, bold);
      }

      > .description {
        font-size: var(--sf-command-form-description-font-size, initial);
        display: var(--sf-command-form-description-display, inherit);
        font-weight: var(--sf-command-form-description-font-weight, initial);
        padding: var(--sf-command-form-description-padding, 1rem 0);
      }
      
      > .error {
        background: var(--sf-command-form-error-background, red);
        color: var(--sf-command-form-error, white);
        padding: var(--sf-command-form-error-padding,  0.5rem 1rem);
      }
      
      .parameters {
        display: grid;
        grid-template: var(--sf-command-form-grid-template, "a b c" auto / minmax(6rem, auto) auto 1fr);
        grid-auto-rows: auto;
        padding: var(--sf-command-form-parameters-padding);
        
        gap: var(--sf-command-form-grid-gap, 0.5rem);

        > .parameter {
          display: contents;

          &.required {
            > .name::after {
              content: '*';
              color: var(--sf-command-form-parameter-required-color, red);
            }
          }

          > .name {
            grid-column: var(--sf-command-form-parameter-name-grid-column, 1);
            padding: var(--sf-command-form-parameter-name-padding, initial);
            justify-self: var(--sf-command-form-parameter-name-alignment, right);
            font-size: var(--sf-command-form-parameter-name-font-size, initial);
            font-weight: var(--sf-command-form-parameter-name-font-weight, initial);
          }

          > .value {
            grid-column: var(--sf-command-form-parameter-value-grid-column, 2);
            
            > input {
              padding: var(--sf-command-form-parameter-value-padding, initial);
              font-size: var(--sf-command-form-parameter-value-font-size, initial);
              width: var(--sf-command-form-parameter-value-width, 100%);
              border: var(--sf-command-form-parameter-value-border, revert);
            }
            
            .value-yes,
            .value-no {
              display: inline-flex;
              padding: var(--sf-command-form-parameter-value-padding, initial);
              font-size: var(--sf-command-form-parameter-value-font-size, initial);
              width: var(--sf-command-form-parameter-value-width, 100%);
              
              > input {
                margin-right: 1rem;
              }
            }
          }

          > .description {
            grid-column: var(--sf-command-form-parameter-description-grid-column, 3);
            padding: var(--sf-command-form-parameter-description-padding, initial);
            font-size: var(--sf-command-form-parameter-description-font-size, initial);
            font-style: var(--sf-command-form-parameter-description-font-style, initial);
            font-weight: var(--sf-command-form-parameter-description-font-weight, initial);
          }
        }
      }
      
      .btn-submit {
        display: var(--sf-command-form-submit-display);
        color: var(--sf-command-form-submit-color);
        background: var(--sf-command-form-submit-background, revert);
        padding: var(--sf-command-form-submit-padding, 0.5rem 1.5rem);
        margin: var(--sf-command-form-submit-margin);
        font-size: var(--sf-command-form-submit-font-size, 1rem);
        font-weight: var(--sf-command-form-submit-font-weight, 500);
        border-radius: var(--sf-command-form-submit-border-radius, 1rem);
        border: var(--sf-command-form-submit-border, 1px solid transparent);
      }

      .btn-reset {
        display: var(--sf-command-form-reset-display);
        color: var(--sf-command-form-reset-color);
        background: var(--sf-command-form-reset-background, revert);
        padding: var(--sf-command-form-reset-padding, 0.5rem 1.5rem);
        margin: var(--sf-command-form-reset-margin);
        font-size: var(--sf-command-form-reset-font-size, 1rem);
        font-weight: var(--sf-command-form-reset-font-weight, 500);
        border-radius: var(--sf-command-form-reset-border-radius, 1rem);
        border: var(--sf-command-form-reset-border, 1px solid transparent);
      }
    }
</style>