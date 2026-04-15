<script lang="ts">
  export let name: string = "World";
  let count = 0;
</script>

<div class="hello-page">
  <h2>Hello Page</h2>
  <p>Hello, {name}! This is the HelloPage view from hello-plugin.</p>
  <button on:click={() => count++}>Clicked {count} times</button>
</div>

<style>
  .hello-page {
    padding: 1rem;
    border: 1px solid #ddd;
    border-radius: 8px;
    background: #f9f9f9;
  }
  button {
    padding: 0.5rem 1rem;
    border-radius: 0.5rem;
    border: none;
    background: royalblue;
    color: white;
    cursor: pointer;
  }
</style>
