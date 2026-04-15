<script lang="ts">
  import { writable } from 'svelte/store';

  // local state for demo purposes
  const darkMode = writable(false);

  function toggle() {
    darkMode.update(v => !v);
  }
</script>

<div class="settings-panel">
  <h2>Settings Panel</h2>

  <label>
    <input type="checkbox" bind:checked={$darkMode} on:change={toggle} />
    Enable Dark Mode
  </label>

  <p>Status: { $darkMode ? "Dark" : "Light" }</p>
</div>

<style>
  .settings-panel {
    padding: 1rem;
    border: 1px solid #ccc;
    border-radius: 8px;
    background: #fff;
  }
</style>
