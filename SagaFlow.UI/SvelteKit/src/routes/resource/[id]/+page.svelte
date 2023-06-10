<script lang="ts">
    import ResourceList from "$lib/ResourceList.svelte";
    export let data;
</script>

<ResourceList id={data.id} />
