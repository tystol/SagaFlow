<svelte:options customElement="sf-not-found"/>
<script lang="ts">
  let count: number = 0
  const increment = () => {
    count += 1
  }
</script>

<button on:click={increment}>
  Not Found count is {count}
</button>