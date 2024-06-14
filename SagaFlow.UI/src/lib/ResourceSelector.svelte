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
        <span class="loading">loading ...</span>
    {:then resources}
        <select id={parameterId} name={parameterId} required={parameter.required} {disabled}>
            <option selected></option>
           
            {#each resources as resource}
                <option value={resource.id}>{resource.name}</option>
            {/each}
        </select>
    {:catch err}
        <span class="error">{err}</span>
    {/await}
{/if}

<style lang="scss">
    select {
      padding: var(--sf-command-form-parameter-value-padding, initial);
      font-size: var(--sf-command-form-parameter-value-font-size, initial);
      width: var(--sf-command-form-parameter-value-width, 100%);
      border: var(--sf-command-form-parameter-value-border, revert);
    }
    
    .loading {
      color: var(--sf-command-form-loading, initial);;
    }
    
    .error {
      background: var(--sf-command-form-error-background, red);
      color: var(--sf-command-form-error, white);
      padding: var(--sf-command-form-error-padding,  1rem 0.5rem);
    }
</style>