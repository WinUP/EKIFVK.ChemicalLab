import { browser, element, by } from 'protractor';

export class EKIFVK.DeusLegem.CreationSystemControlPanelPage {
  navigateTo() {
    return browser.get('/');
  }

  getParagraphText() {
    return element(by.css('system-root h1')).getText();
  }
}
