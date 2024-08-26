<script>
    import { addErrorToast } from "@/stores/toasts";
    import ApiClient from "@/utils/ApiClient";
    import OverlayPanel from "@/components/base/OverlayPanel.svelte";
    import CopyIcon from "@/components/base/CopyIcon.svelte";
    import FormattedDate from "@/components/base/FormattedDate.svelte";
    import RecordFieldValue from "@/components/records/RecordFieldValue.svelte";
    import Accordion from "@/components/base/Accordion.svelte";

    export let collection;

    let recordPanel;
    let record = {};
    let isLoading = false;

    $: hasEditorField = !!collection?.schema?.find((f) => f.type === "editor");

    export function show(model) {
        load(model);

        return recordPanel?.show();
    }

    export function hide() {
        isLoading = false;
        return recordPanel?.hide();
    }

    async function load(model) {
        record = {}; // reset

        isLoading = true;

        record = (await resolveModel(model)) || {};

        isLoading = false;
    }

    async function resolveModel(model) {
        if (model && typeof model === "string") {
            // load from id
            try {
                return await ApiClient.collection(collection.id).getOne(model);
            } catch (err) {
                if (!err.isAbort) {
                    hide();
                    console.warn("resolveModel:", err);
                    addErrorToast(`Unable to load record with id "${model}"`);
                }
            }

            return null;
        }

        return model;
    }
</script>

<OverlayPanel
    bind:this={recordPanel}
    class="record-preview-panel {hasEditorField ? 'overlay-panel-xl' : 'overlay-panel-lg'}"
    on:hide
    on:show
>
    <svelte:fragment slot="header">
        <h4><strong>{collection?.name}</strong> status</h4>
    </svelte:fragment>

    <h4 class="section-title">Command</h4>

    <table class="table-border preview-table" class:table-loading={isLoading}>
        <tbody>
            <tr>
                <td class="min-width txt-hint txt-bold">id</td>
                <td class="col-field">
                    <div class="label">
                        <CopyIcon value={record.id} />
                        <span class="txt">{record.id || "..."}</span>
                    </div>
                </td>
            </tr>

            {#each collection?.schema as field}
                <tr>
                    <td class="min-width txt-hint txt-bold">{field.name}</td>
                    <td class="col-field">
                        <RecordFieldValue {field} {record} />
                    </td>
                </tr>
            {/each}

            {#if record.created}
                <tr>
                    <td class="min-width txt-hint txt-bold">created</td>
                    <td class="col-field"><FormattedDate date={record.created} /></td>
                </tr>
            {/if}

            {#if record.updated}
                <tr>
                    <td class="min-width txt-hint txt-bold">updated</td>
                    <td class="col-field"><FormattedDate date={record.updated} /></td>
                </tr>
            {/if}
        </tbody>
    </table>

    {#if record.handlers?.length > 0}
    <br />
    <h4 class="section-title">Handlers</h4>

    <div class="accordions">
        {#each record.handlers as commandHandler}
        <Accordion single>
            <svelte:fragment slot="header">
                <div class="inline-flex">
                    <i class="ri-user-star-line" />
                    <span class="txt">{commandHandler.name}</span>
                </div>

                <div class="flex-fill" />
                <span class="label" class:label-success={commandHandler.status === "Complete"}>{commandHandler.status}</span>
                {#if commandHandler.errors?.length > 0}
                    <i
                        class="ri-error-warning-fill txt-danger"
                        transition:scale={{ duration: 150, start: 0.7 }}
                        use:tooltip={{ text: "Has errors", position: "left" }}
                    />
                {/if}
            </svelte:fragment>

            <pre>{JSON.stringify(commandHandler, undefined, 2)}</pre>
        </Accordion>
        {/each}
    </div>
    {/if}

    {#if record.relatedSagas?.length > 0}
    <br />
    <h4 class="section-title">Sagas</h4>

    <div class="accordions">
        {#each record.relatedSagas as saga}
        <Accordion single>
            <svelte:fragment slot="header">
                <div class="inline-flex">
                    <i class="ri-user-star-line" />
                    <span class="txt">{saga.name}</span>
                </div>

                <div class="flex-fill" />
                <span class="label" class:label-success={saga.status === "Complete"}>{saga.status}</span>
                {#if saga.errors?.length > 0}
                    <i
                        class="ri-error-warning-fill txt-danger"
                        transition:scale={{ duration: 150, start: 0.7 }}
                        use:tooltip={{ text: "Has errors", position: "left" }}
                    />
                {/if}
            </svelte:fragment>

            <pre>{JSON.stringify(saga, undefined, 2)}</pre>
        </Accordion>
        {/each}
    </div>
    {/if}
    

    <br />
    <pre>{JSON.stringify(record, undefined, 2)}</pre>

    <svelte:fragment slot="footer">
        <button type="button" class="btn btn-transparent" on:click={() => hide()}>
            <span class="txt">Close</span>
        </button>
    </svelte:fragment>
</OverlayPanel>

<style lang="scss">
    .col-field {
        max-width: 1px; // text overflow workaround
    }
</style>
