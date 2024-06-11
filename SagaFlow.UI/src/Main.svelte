<svelte:options customElement={{
    tag: "sf-main",
    shadow: "none",
    extend: extendToGetWebComponentRoot
}}/>
<script lang="ts">
  import Router from 'svelte-spa-router'
  import routes from './routes';
  import {setContext} from "svelte";
  import {type Readable, writable, type Writable} from "svelte/store";
  import sagaFlow, {defaultSagaFlowServer, type ISagaFlowServerState} from "./state/SagaFlowState";
  import type {Config, Setup} from "$lib/types";
  import {
      createWebComponentEventDispatcher,
      extendToGetWebComponentRoot
  } from "$lib/RootWebComponentEventDispatcher";
  
  export let __webComponentElement: HTMLElement | undefined = undefined;
  
  export let serverKey: string = defaultSagaFlowServer;
  
  let setup: Setup;
  let config: Config;
  
  const title = setContext<Writable<string>>("title", writable<string>());
  setContext<string>("serverKey", serverKey);
  
  createWebComponentEventDispatcher(__webComponentElement);
  
  let store: Readable<ISagaFlowServerState>;
  
  $: store = sagaFlow.state(serverKey);
  $: setup = $store.setup;
  $: config = $store.config;
</script>


<svelte:head>
    <title>{[$title, "SagaFlow"].filter(v => v).join(" • ")}</title>
</svelte:head>

<div class="saga-flow-main">
    <header>
        <a href={`${setup.baseUrl}/${setup.webRoot}`}>SagaFlow</a>
        <span>
            Commands
            {#each Object.entries(config.commands) as [id, command]}
                <a href={`#/commands/${id}`}>{command.name}</a>
            {/each}
        </span>
        <span>
            Resources
            {#each Object.entries(config.resourceLists) as [id,resource]}
                <a href={`#/resources/${id}`}>{resource.name}</a>
            {/each}
        </span>
    </header>
    <main>
        <Router {routes}/>
    </main>
    <footer>
        SagaFlow
    </footer>
</div>
<style>
    :global(html, body) {
        padding: 0;
        margin: 0;
    }
    
    .saga-flow-main {
        display: grid;
        grid-template:
            "header" auto
            "main" 1fr
            "footer" auto
            / 1fr;
        height: 100%;
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