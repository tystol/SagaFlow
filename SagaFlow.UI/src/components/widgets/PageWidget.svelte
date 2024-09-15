<script>
	import { tick, onMount } from 'svelte';
    import PageWrapper from "@/components/base/PageWrapper.svelte";
    import sagaFlow, {defaultSagaFlowServer} from "@/state/SagaFlowState";

    export let params = {};

    var sf = sagaFlow.state();

    let widgetRootElement;
    let isWidgetLoading = true;
    let widgetComponent;

    $: if (params.id && widgetRootElement){
        loadWidget();
    }

    async function loadWidget() {
        isWidgetLoading = true;
        widgetRootElement.replaceChildren();
        var widget = $sf.config.sidebarWidgets[params.id];
        const widgetModule = await import(/* @vite-ignore */ widget.href);
        var newWidget = document.createElement(widget.webComponentId);
        newWidget.setAttribute('name', 'SagaFlow');
        widgetRootElement.appendChild(newWidget);
        isWidgetLoading = false;
    }
</script>

<PageWrapper center={isWidgetLoading} class={!isWidgetLoading ? 'flex-content' : ''}>
    {#if isWidgetLoading}
        <div class="placeholder-section m-b-base">
            <span class="loader loader-lg" />
            <h1>Loading...</h1>
        </div>
    {/if}
    <div bind:this={widgetRootElement}></div>
</PageWrapper>
