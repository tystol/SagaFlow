<script>
    import { createEventDispatcher } from "svelte";
    import ApiClient from "../../utils/ApiClient";
    import CommonHelper from "@/utils/CommonHelper";

    const dispatch = createEventDispatcher();

    export let type;
    export let collection;
    export let filter = "";
    export let totalCount = 0;

    let classes = undefined;
    export { classes as class }; // export reserved keyword

    let isLoading = false;

    $: if (collection?.id && filter !== -1) {
        if (type === collection?.type){
            reload();
        }
    }

    export async function reload() {
        if (!collection?.id) {
            return;
        }

        isLoading = true;
        totalCount = 0;

        try {
            const fallbackSearchFields = CommonHelper.getAllCollectionIdentifiers(collection);

            // TODO: don't query server again when not using paginated providers. obtain total items count from 1st query that loaded all data.
            let dataFetch = collection.type === 'resources' ?
                ApiClient.resourceList(collection.id).getList(1, 1, {
                    filter: CommonHelper.normalizeSearchFilter(filter, fallbackSearchFields),
                    fields: "id",
                    requestKey: "records_count",
                })
                :
                ApiClient.command(collection.id).getHistory(1, 1, {
                    filter: CommonHelper.normalizeSearchFilter(filter, fallbackSearchFields),
                    fields: "id",
                    requestKey: "records_count",
                });
            let result = await dataFetch;

            totalCount = result.totalItems;
            dispatch("count", totalCount);
            isLoading = false;
        } catch (err) {
            if (!err?.isAbort) {
                isLoading = false;
                console.warn(err);
            }
        }
    }
</script>

<div class="inline-flex flex-gap-5 records-counter {classes}">
    <span class="txt">Total found:</span>
    <span class="txt">{!isLoading ? totalCount : "..."}</span>
</div>
