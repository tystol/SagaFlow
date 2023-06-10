<script lang="ts">
    import {getContext} from "svelte";
    import type {Config, Resource, ResourceList, Setup} from "./types";
    import {get, writable, type Writable} from "svelte/store";

    const setup = getContext<Setup>("setup");
    const config = getContext<Config>("config");
    const cache = getContext<Record<string, Writable<Array<Resource>>>>("resource-cache");

    export let id: string = "";
    
    let resourceListDefinition: ResourceList;
    $: resourceListDefinition = config.resourceLists[id];

    let resources: Promise<Writable<Array<Resource>>>;
    $: resources = loadResourceList(resourceListDefinition, id);

    const loadResourceList = async (rld: ResourceList, resourceListId: string) => {
        if (!rld) {
            return Promise.reject(`Resource List ${resourceListId} does not exist`);
        }
        if (cache[resourceListId]) {
            return get(cache[resourceListId]);
        }
        const results = await fetch(`${setup.apiRoot}/${rld.href}`);
        if (results.ok) {
            const data = await results.json();
            cache[resourceListId] = writable<Array<Resource>>(data);
            return data;
        }
    };
    
</script>

<style>
    .name {
        font-size: 1.125rem;
        font-weight: bold;
    }
</style>

{#if $$slots.default}
    {#await resources then resources}
        {#each resources as resource}
            <slot id={resource.id} title={resource.name} />
        {/each}
    {/await}
{:else}
    {#await resources}
        Loading...
    {:then resources}
        <span class="name">Resource List: {resourceListDefinition.name}</span>
        <ol>
            {#each resources as resource}
                <li>{resource.id}: {resource.name}</li>
            {/each}
        </ol>
    {:catch err}
        {err}
    {/await}
{/if}
