<script>
	import { tick, onMount } from 'svelte';
    import PageWrapper from "@/components/base/PageWrapper.svelte";

    export let widgetUrl;

    let widgetRootElement;
    let isWidgetLoading = true;

    async function loadWidget() {
        isWidgetLoading = true;
        const widgetModule = await import(/* @vite-ignore */ widgetUrl);
        const createWidget = widgetModule.default;
        await tick(); // ensure widgetRootElement binding in place.
        createWidget(widgetRootElement)
        isWidgetLoading = false;
    }

    onMount(loadWidget);
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
