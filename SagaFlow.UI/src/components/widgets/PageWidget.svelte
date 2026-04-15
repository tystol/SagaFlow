<script>
	import { tick, onMount } from 'svelte';
    import PageWrapper from "@/components/base/PageWrapper.svelte";
    import sagaFlow, {defaultSagaFlowServer} from "@/state/SagaFlowState";

    export let params = {};

    var sf = sagaFlow.state();

    let isWidgetLoading = true;
    let widgetComponent;

    $: if (params.id){
        loadWidget();
    }

    async function loadWidget() {
        isWidgetLoading = true;

        /* TODO: add support for web components.
        widgetRootElement.replaceChildren();
        */

        var widget = $sf.config.sidebarComponents[params.id];
        const pluginModule = await import(/* @vite-ignore */ widget.href);
        const pluginManifest = pluginModule.manifest;
        widgetComponent = pluginManifest.views[widget.webComponentId];

        /* TODO: add support for web components.
        var newWidget = document.createElement(pluginManifest.views[widget.webComponentId]);
        newWidget.setAttribute('name', 'SagaFlow');
        widgetRootElement.appendChild(newWidget);
        */
        isWidgetLoading = false;
    }
</script>

<PageWrapper center={isWidgetLoading} class={!isWidgetLoading ? 'flex-content' : ''}>
    {#if isWidgetLoading}
        <div class="placeholder-section m-b-base">
            <span class="loader loader-lg" />
            <h1>Loading...</h1>
        </div>
    {:else}
        {#if widgetComponent}
            <svelte:component this={widgetComponent} />
        {:else}
            <div class="placeholder-section m-b-base">
                <h1>Widget not found</h1>
                <p>The requested widget could not be found.</p>
            </div>
        {/if}
    {/if}
    <!-- TODO: add support for web components.
    <div bind:this={widgetRootElement}></div>
    -->
</PageWrapper>
