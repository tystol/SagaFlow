<script lang="ts">
    import {getContext} from "svelte";
    import ResourceList from "$lib/ResourceList.svelte";
    import type {Command, Config, Setup} from "$lib/types";
	import type { PageData } from "./$types";

    export let data: PageData;

    const setup = getContext<Setup>("setup");
    const config = getContext<Config>("config");
    
    let commandDefinition: Command;
    $: commandDefinition = config.commands[data.id];

    let form: HTMLFormElement;

    async function sendCommand() {

        var command = Object.fromEntries(new FormData(form).entries());

        await fetch(`${setup.apiRoot}/${commandDefinition.href}`, {
            method: "POST",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(command),
        }).catch(e => {
            console.error(e);
        });
    }
</script>

<style>
    .command > .name {
        font-size: 1.125rem;
        font-weight: bold;
    }

    .parameters {
        display: grid;
        grid-template-columns: minmax(6rem, auto) auto 1fr;
        grid-auto-rows: 1fr;
        gap: 0.5rem;
    }

    .parameters > .parameter {
        display: contents;
    }

    .parameter > .name {
        grid-column: 1;
        justify-self: right;
        align-self: center;
    }

    .parameter > .value {
        grid-column: 2;
    }

    .parameter > .description {
        grid-column: 3;
        align-self: center;
    }

    .required {
        color: red;
    }
</style>

<!--
{JSON.stringify(data)}
<br/>
<br/>
{JSON.stringify(command)}
-->

{#if commandDefinition}
<form class="command" bind:this={form}>
    <div class="name">Command: {commandDefinition.name}</div>
    <div>Parameters</div>
    <div class="parameters">
        {#each Object.entries(commandDefinition.parameters) as [parameterId, parameter]}
            {@const resourceList = parameter.resourceListId ? config.resourceLists[parameter.resourceListId] : null}
            <div class="parameter">
                <span class="name">{parameterId}</span>
                <span class="value">
                    {#if parameter.type === "String"}
                        <input type="text" name={parameterId} required={parameter.required} />
                    {:else if parameter.type === "Boolean"}
                        <label><input type="radio" value={true} group={parameterId}> Yes</label>
                        <label><input type="radio" value={false} group={parameterId}> No</label>
                    {:else if resourceList}
                        <select name={parameterId} required={parameter.required}>
                                <option selected disabled={parameter.required}></option>
                            <ResourceList id={parameter.resourceListId} let:id={resourceId} let:title={resourceTitle}>
                                <option value={resourceId}>{resourceTitle}</option>
                            </ResourceList>
                        </select>
                    {:else}
                        {JSON.stringify({resourceList})}
                    {/if}
                    {#if parameter.required}<small class="required">Required</small>{/if}
                </span>
                {#if parameter.description}<small class="description">{parameter.description}</small>{/if}
                
            </div>
        {/each}
    </div>
    <button on:click={sendCommand}>Run</button>
</form>
{/if}