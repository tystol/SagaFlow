<script lang="ts">
    import ResourceList from "$lib/ResourceList.svelte";
    export let params = {id:""}
</script>

<ResourceList id={params.id} />
