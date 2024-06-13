<svelte:options customElement={{
    tag: "sf-command-selector",
    shadow: "none",
    extend: extendToGetWebComponentRoot
}} />
<script lang="ts">

    import sagaFlow, {defaultSagaFlowServer, type ISagaFlowServerState} from "../state/SagaFlowState.js";
    import type {Readable} from "svelte/store";
    import {createEventDispatcher} from "svelte";
    import {
        createWebComponentEventDispatcher,
        extendToGetWebComponentRoot
    } from "$lib/RootWebComponentEventDispatcher";
    
    // When used as web component
    export let __webComponentElement: HTMLElement | undefined = undefined;    
    export let serverKey: string = defaultSagaFlowServer;
    export let label: string = "Command";
    export let noSelectionDisplay: string = "Select ...";
    
    const dispatcher = createWebComponentEventDispatcher(__webComponentElement);
    
    export let commandId: string | undefined = undefined;
    
    let store: Readable<ISagaFlowServerState>;
    
    const onCommandSelected = (commandId: string) => dispatcher("sf-command-selected", { serverKey, commandId, command: $store.config.commands[commandId] });
    
    $: store = sagaFlow.state(serverKey);
    
    $: onCommandSelected(commandId);
</script>

<div class="command-selector">
    <label for="command-selector">{label}</label>
    <div class="value">
        <select id="command-selector" bind:value={commandId}>
            <option selected value={undefined}>{noSelectionDisplay}</option>

            {#each Object.entries($store.config.commands) as [commandId, command]}
                <option value={commandId}>{command.name}</option>
            {/each}
        </select>
    </div>
</div>
