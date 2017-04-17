import { EKIFVK.DeusLegem.CreationSystemControlPanelPage } from './app.po';

describe('ekifvk.deus-legem.creation-system-control-panel App', () => {
  let page: EKIFVK.DeusLegem.CreationSystemControlPanelPage;

  beforeEach(() => {
    page = new EKIFVK.DeusLegem.CreationSystemControlPanelPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('system works!');
  });
});
