<script lang="ts">
    import type {Parameter, Resource} from "$lib/Models";
    import sagaFlow from "../state/SagaFlowState";

    export let serverKey: string;
    export let parameterId: string;
    export let parameter: Parameter;
    export let disabled: boolean = false;

    let resources: Resource[] | undefined = [];
    
    const loadResourceSuggestions = async (resourceId: string, serverKey: string) => {
        resources = await sagaFlow.getResources(resourceId, serverKey)
    }

    $: if (parameter.resourceListId) 
        loadResourceSuggestions(parameter.resourceListId, serverKey);
    
</script>

<input type="text" id={parameterId} name={parameterId} required={parameter.required} disabled={disabled} list={resources ? `${parameterId}-list` : undefined} />

{#if resources}
    <datalist id={`${parameterId}-list`}>
        {#each resources as resource}
            <option value={resource.name} />
        {/each}
    </datalist>
{/if}

<style lang="scss">
  input {
    padding: var(--sf-command-form-parameter-value-padding, initial);
    font-size: var(--sf-command-form-parameter-value-font-size, initial);
    width: var(--sf-command-form-parameter-value-width, 100%);
    border: var(--sf-command-form-parameter-value-border, revert);
  }
</style>