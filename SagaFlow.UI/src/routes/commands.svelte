<script lang="ts">
    import {getContext} from "svelte";
    import CommandForm from "$lib/CommandForm.svelte";

    export let params = {id:""}
    
    const serverKey = getContext<string>("serverKey");
</script>

<CommandForm commandId={params.id} {serverKey} />