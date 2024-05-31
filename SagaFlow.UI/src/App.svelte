<script lang="ts">
  import type {Setup, Config} from "$lib/types";
  import {setContext} from "svelte";
  import Main from './Main.svelte';
  
  export let rootPath: string = "";

  const setup: Setup = {
      apiRoot: "",
      webRoot: "/",
  };
  setup.apiRoot = setup.apiRoot.replace(/\/$/, "");
  setup.webRoot = setup.webRoot.replace(/\/$/, "");

  const fetchConfig: Promise<Config> = fetch(`${rootPath}/schema`).then(result => result.ok ? result.json() : Promise.reject(result)) as Promise<Config>
  
  setContext<Setup>("setup", setup);

</script>

{#await fetchConfig}
  Loading...
{:then config}
  <Main {config}></Main>
{:catch error}
  <p>Something went wrong: {error.message}</p>
{/await}
