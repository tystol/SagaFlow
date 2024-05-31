<svelte:options customElement="sf-resource-selector"/>
<script lang="ts">
    import sagaFlow from "../state/SagaFlowState";
    import type {Parameter, Resource} from "$lib/types";
    
    export let serverKey: string;
    export let resourceId: string;
    export let parameterId: string;
    export let parameter: Parameter;
    export let disabled: boolean = false; 
    
    let resources:Promise<Resource[]>;
    
    $: if (resourceId) resources = sagaFlow.getResources(resourceId, serverKey);
</script>

{#if resources}
    {#await resources}
        <span>loading ...</span>
    {:then resources}
        <select name={parameterId} required={parameter.required} {disabled}>
            <option selected></option>
           
            {#each resources as resource}
                <option value={resource.id}>{resource.name}</option>
            {/each}
        </select>
    {:catch err}
        {err}
    {/await}
{/if}