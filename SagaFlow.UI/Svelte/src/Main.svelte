<script lang="ts">
  import Router from 'svelte-spa-router'
  import routes from './routes';
  import type {Setup, Config, Resource} from "$lib/types";
  import {getContext,setContext} from "svelte";
  import {writable, type Writable} from "svelte/store";

  export let config: Config;

  const setup = getContext<Setup>("setup");
  setContext<Config>("config", config);
  setContext<Record<string, Writable<Resource>>>("resource-cache", {});
  const title = setContext<Writable<string>>("title", writable<string>());

</script>


<svelte:head>
    <title>{[$title, "SagaFlow"].filter(v => v).join(" • ")}</title>
</svelte:head>

<header>
    <a href={setup.webRoot}>SagaFlow</a>
    <span>
        Commands
        {#each Object.entries(config.commands) as [id, command]}
            <a href={`#${setup.webRoot}/commands/${id}`}>{command.name}</a>
        {/each}
    </span>
    <span>
        Resources
        {#each Object.entries(config.resourceLists) as [id,resource]}
            <a href={`#${setup.webRoot}/resources/${id}`}>{resource.name}</a>
        {/each}
    </span>
</header>
<main>
    <Router {routes}/>
</main>
<footer>
    SagaFlow
</footer>



<style>
    :global(html, body) {
        padding: 0;
        margin: 0;
    }
    :global(body) {
        display: grid;
        grid-template:
            "header" auto
            "main" 1fr
            "footer" auto
            / 1fr;
        height: 100vh;
    }
    header {
        grid-area: header;
        display: flex;
        flex-direction: row;
        padding: 0.25rem;
        gap: 0.25rem;
        border-bottom: solid 1px black;
    }
    header > * {
        background-color: lightblue;
        border-radius: 0.5rem;
        margin: 0;
    }
    header > span {
        padding: 0.5rem;
    }
    header > span > :last-child {
        border-bottom-right-radius: 0.5rem;
        border-top-right-radius: 0.5rem;
        margin-right: -0.5rem;
    }
    header a {
        background-color: lightsteelblue;
        color: black;
        text-decoration: none;
        padding: 0.5rem;
    }

    main {
        grid-area: main;
        padding: 1rem;
    }

    footer {
        background-color: white;
        grid-area: footer;
        position: sticky;
        inset: auto 0 0 0;
        padding: 0.5rem;
        border-top: solid 1px black;
    }
</style>