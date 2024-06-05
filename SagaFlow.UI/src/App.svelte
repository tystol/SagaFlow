<script lang="ts">
  import Main from './Main.svelte';
  import sagaFlow from "./state/SagaFlowState";
  

  let error: Error | undefined = undefined;
  
  // setContext<Setup>("setup", setup);
  
  try {
    sagaFlow.initialize()  
  }
  catch (err)
  {
    error = err;
  }

</script>


{#if error}
  <p>Something went wrong: {error.message}</p>
{:else}
    <Main />
{/if}