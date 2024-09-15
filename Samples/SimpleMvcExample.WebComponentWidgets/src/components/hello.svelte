<svelte:options customElement="hello-world" />

<script>
  import Counter from "../lib/Counter.svelte"
  export let name = "World";


</script>

<div>Hello {name}</div>
<Counter></Counter>

