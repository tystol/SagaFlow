<script lang="ts">
    export let maxNumberOfPageSlots: number = 5;
    
    export let pageIndex: number;
    
    export let pageSize: number;
    
    export let total: number;
    
    let hasNextPage: boolean;
    
    let hasPrevPage: boolean;
    
    let pageSlotStart: number = 0;
    
    let pageSlotEnd: number = 0;
    
    const gotoPrevious = () => pageIndex --;

    const gotoNext = () => pageIndex ++;
    
    const gotoPage = (index: number) => pageIndex = index;
    
    const updatePageSlots = (maxNumberOfPageSlots: number,  pageIndex: number, pageSize: number, total: number) => {
        const totalPages = Math.ceil(total / pageSize); 
        
        pageSlotStart = Math.max(0, pageIndex - Math.floor((maxNumberOfPageSlots / 2)));
        
        pageSlotEnd = Math.min(totalPages, pageSlotStart + maxNumberOfPageSlots);
        
        if (pageSlotEnd - pageSlotStart < maxNumberOfPageSlots)
        {
            pageSlotStart = Math.max(0, pageSlotEnd - (maxNumberOfPageSlots));
        }
    }
    
    $: hasPrevPage = pageIndex > 0;
    
    $: hasNextPage = ((pageIndex + 1) * pageSize) < total;
    
    $: updatePageSlots(maxNumberOfPageSlots, pageIndex, pageSize, total);
</script>

<div>
    <button class="paging-btn previous"
            class:previous-none={!hasPrevPage}
            on:click={gotoPrevious} disabled={!hasPrevPage}>Previous</button>

    {#each Array(pageSlotEnd - pageSlotStart) as _, index}
        {@const page = pageSlotStart + index}
        <button class="paging-btn"
                class:current-page={page === pageIndex}
                on:click={() => gotoPage(page)}>{page + 1}</button>
    {/each}

    <button class="paging-btn next"
            class:next-none={!hasNextPage}
            on:click={gotoNext} disabled={!hasNextPage}>Next</button>
</div>

<style lang="scss">
    .paging-btn {
      padding: var(--sf-paging-padding, 0.25rem);
      margin: var(--sf-paging-margin, 0);
      border: var(--sf-paging-border, 1px solid black);
      font: var(--sf-paging-font, inherit);
      
      &.current-page {
        font-style: var(--sf-paging-current-font-style, inherit);
        font-weight: var(--sf-paging-current-font-weight, bold);
      }
    }
</style>