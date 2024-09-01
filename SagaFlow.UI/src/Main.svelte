<svelte:options customElement={{
    tag: "sf-main",
    shadow: "none",
    extend: extendToGetWebComponentRoot
}}/>
<script lang="ts">

    import 'remixicon/fonts/remixicon.css'
    import "./scss/main.scss";

  import Router, { replace, link } from "svelte-spa-router";
  import routes from './routes';
  import active from "svelte-spa-router/active";
  import tooltip from "./actions/tooltip";
  import CommonHelper from "./utils/CommonHelper";
  import Toggler from "./components/base/Toggler.svelte";
  import Confirmation from "./components/base/Confirmation.svelte";
  import Toasts from "./components/base/Toasts.svelte";
  import { resetConfirmation } from "./stores/confirmation";
  import { setErrors } from "./stores/errors";
  import {setContext} from "svelte";
  import {type Readable, writable, type Writable} from "svelte/store";
  import sagaFlow, {defaultSagaFlowServer, type ISagaFlowServerState} from "./state/SagaFlowState";
  import type {Config, Setup} from "$lib/Models";
  import {
      createWebComponentEventDispatcher,
      extendToGetWebComponentRoot
  } from "$lib/RootWebComponentEventDispatcher";
  import logo from "$assets/logo.svg?raw";
  const logoEncoded = `data:image/svg+xml,${encodeURIComponent(logo)}`;
  
  export let __webComponentElement: HTMLElement | undefined = undefined;
  
  export let serverKey: string = defaultSagaFlowServer;
  
  let oldLocation = undefined;
  let showAppSidebar = true;
  
  const title = setContext<Writable<string>>("title", writable<string>());

  function handleRouteLoading(e) {
        if (e?.detail?.location === oldLocation) {
            return; // not an actual change
        }

        //showAppSidebar = !!e?.detail?.userData?.showAppSidebar;

        oldLocation = e?.detail?.location;

        // resets
        $title = "";
        setErrors({});
        resetConfirmation();
    }

    function handleRouteFailure() {
        replace("/");
    }

  function logout() {
    //ApiClient.logout();
  }

  let setup: Setup;
  let config: Config;
  
  setContext<string>("serverKey", serverKey);
  
  createWebComponentEventDispatcher(__webComponentElement);
  
  let store: Readable<ISagaFlowServerState>;

    
  replace("/collections");
  
  $: store = sagaFlow.state(serverKey);
  $: setup = $store.setup;
  $: config = $store.config;
</script>


<svelte:head>
    <title>{CommonHelper.joinNonEmpty([$title, "SagaFlow"], " - ")}</title>
</svelte:head>


<div class="app-layout">
    {#if showAppSidebar}
        <aside class="app-sidebar">
            <a href="/" class="logo logo-sm" use:link use:tooltip={{ text: "SagaFlow", position: "right" }}>
                <!-- <i class="ri-flow-chart" style="font-size: 3rem;"/> -->
                <img
                    src={logoEncoded}
                    alt="SagaFlow logo"
                    width="40"
                    height="40"
                />
            </a>

            <nav class="main-menu">
                <a
                    href="/collections"
                    class="menu-item"
                    aria-label="Collections"
                    use:link
                    use:active={{ path: "/collections/?.*", className: "current-route" }}
                    use:tooltip={{ text: "Collections", position: "right" }}
                >
                    <i class="ri-database-2-line" />
                </a>
                <a
                    href="/commands"
                    class="menu-item"
                    aria-label="Commands"
                    use:link
                    use:active={{ path: "/commands/?.*", className: "current-route" }}
                    use:tooltip={{ text: "Commands", position: "right" }}
                >
                    <i class="ri-function-add-line" />
                </a>
                <a
                    href="/logs"
                    class="menu-item"
                    aria-label="Logs"
                    use:link
                    use:active={{ path: "/logs/?.*", className: "current-route" }}
                    use:tooltip={{ text: "Logs", position: "right" }}
                >
                    <i class="ri-line-chart-line" />
                </a>
                <a
                    href="/settings"
                    class="menu-item"
                    aria-label="Settings"
                    use:link
                    use:active={{ path: "/settings/?.*", className: "current-route" }}
                    use:tooltip={{ text: "Settings", position: "right" }}
                >
                    <i class="ri-tools-line" />
                </a>
            </nav>

            <div
                tabindex="0"
                role="button"
                aria-label="Logged admin menu"
                class="thumb thumb-circle link-hint closable"
            >
                <i class="ri-user-3-line" style="font-size: 2rem;"/>
                <!--
                <img
                    src="{import.meta.env.BASE_URL}images/avatars/avatar{0}.svg"
                    alt="Avatar"
                    aria-hidden="true"
                />
                -->
                <Toggler class="dropdown dropdown-nowrap dropdown-upside dropdown-left">
                    <a href="/settings/admins" class="dropdown-item closable" role="menuitem" use:link>
                        <i class="ri-shield-user-line" aria-hidden="true" />
                        <span class="txt">Manage admins</span>
                    </a>
                    <hr />
                    <button type="button" class="dropdown-item closable" role="menuitem" on:click={logout}>
                        <i class="ri-logout-circle-line" aria-hidden="true" />
                        <span class="txt">Logout</span>
                    </button>
                </Toggler>
            </div>
        </aside>
    {/if}

    <div class="app-body">
        <Router {routes} on:routeLoading={handleRouteLoading} on:conditionsFailed={handleRouteFailure} />

        <Toasts />
    </div>
</div>

<Confirmation />
<!--
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
-->