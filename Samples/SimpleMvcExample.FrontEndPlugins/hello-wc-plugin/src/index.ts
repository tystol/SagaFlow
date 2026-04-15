/// <reference types="svelte" />

import HelloPage from './HelloPage.svelte';
import SettingsPanel from './SettingsPanel.svelte';

export const manifest = {
  name: 'hello-plugin',
  version: '0.1.0',
  views: {
    hello: HelloPage,
    settings: SettingsPanel
  }
};

