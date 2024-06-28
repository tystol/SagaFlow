<svelte:options customElement="sf-resource-list"/>
<script lang="ts">
    import type {ResourceList} from "./Models";
    import sagaFlow, {defaultSagaFlowServer, type ISagaFlowServerState} from "../state/SagaFlowState";
    import type {Readable} from "svelte/store";

    export let resourceId: string = "";
    export let serverKey: string = defaultSagaFlowServer;
    
    let resourceListDefinition: ResourceList | undefined = undefined;
    let store: Readable<ISagaFlowServerState>;
    
    $: store = sagaFlow.state(serverKey);
    $: resourceListDefinition = $store.config.resourceLists[resourceId];
    $: resources = sagaFlow.getResources(resourceId, serverKey);
    
</script>

<style>
    .name {
        font-size: 1.125rem;
        font-weight: bold;
    }
</style>

{#if resources}
    {#await resources}
        Loading...
    {:then resources}
        <span class="name">Resource List: {resourceListDefinition?.name}</span>
        <ol>
            {#each resources as resource}
                <li>{resource.id}: {resource.name}</li>
            {/each}
        </ol>
    {:catch err}
        {err}
    {/await}
{/if}

