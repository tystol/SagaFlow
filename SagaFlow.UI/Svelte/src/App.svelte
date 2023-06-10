<script lang="ts">
  import type {Setup, Config} from "$lib/types";
  import {setContext} from "svelte";
  import Main from './Main.svelte';

  const setup: Setup = {
      apiRoot: "https://localhost:7172",
      apiPath: "sagaflow",
      webRoot: "/",
  };
  setup.apiRoot = setup.apiRoot.replace(/\/$/, "");
  setup.webRoot = setup.webRoot.replace(/\/$/, "");

  const fetchConfig: Promise<Config> = fetch(`${setup.apiRoot}/sagaflow/schema`).then(result => result.ok ? result.json() : Promise.reject(result)) as Promise<Config>
  
  setContext<Setup>("setup", setup);

</script>

{#await fetchConfig}
  Loading...
{:then config}
  <Main {config}></Main>
{/await}
