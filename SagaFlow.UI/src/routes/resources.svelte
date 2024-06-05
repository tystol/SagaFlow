<script lang="ts">
    import ResourceList from "$lib/ResourceList.svelte";
    import {getContext} from "svelte";
    export let params = {id:""}
    
    const serverKey = getContext<string>("serverKey");
</script>

<ResourceList resourceId={params.id} {serverKey} />
