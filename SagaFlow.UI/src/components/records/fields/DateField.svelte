<script>
    import Flatpickr from "svelte-flatpickr";
    import CommonHelper from "@/utils/CommonHelper";
    import Field from "@/components/base/Field.svelte";
    import tooltip from "@/actions/tooltip";

    export let mode = 'datetime';
    export let field;
    export let value = undefined;

    let pickerValue = value;

    // strip ms and zone for backwards compatibility with the older format
    // and because flatpickr currently doesn't have integrated
    // zones support and requires manual parsing and formatting
    $: if (value && value.length > 24) {
        value = value.substring(0, 24);
    }

    $: if (pickerValue != value) {
        pickerValue = value;
    }

    // ensure that value is set even on manual input edit
    function onClose(e) {
        if (e.detail && e.detail.length == 3) {
            value = e.detail[1];
        }
    }

    function clear() {
        value = "";
    }
</script>

<Field class="form-field {field.required ? 'required' : ''}" name={field.name} let:uniqueId>
    <label for={uniqueId}>
        <i class={CommonHelper.getFieldTypeIcon(field.type)} />
        <span class="txt">{field.name}{mode === 'datetime' ? ' (UTC)' : ''}</span>
    </label>

    {#if value && !field.required}
        <div class="form-field-addon">
            <button type="button" class="link-hint clear-btn" use:tooltip={"Clear"} on:click={() => clear()}>
                <i class="ri-close-line" />
            </button>
        </div>
    {/if}

    <Flatpickr
        id={uniqueId}
        options={mode === 'datetime' ? CommonHelper.defaultFlatpickrOptions() : CommonHelper.dateonlyFlatpickrOptions()}
        bind:value={pickerValue}
        bind:formattedValue={value}
        on:close={onClose}
    />
    
    {#if field.description}<div class="help-block">{field.description}</div>{/if}
</Field>

<style>
    .clear-btn {
        margin-top: 20px;
    }
</style>
