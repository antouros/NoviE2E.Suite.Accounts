Feature: User login and logout

  The account holder authenticates against the Novibet site and terminates the session.
  Showcases Playwright + Reqnroll replacing the QA.Novi Selenium stack.

  @UI @smoke @Team_Tanzanite @Feature_Authentication
  Scenario: User logs in and logs out
    Given the user navigates to the site
    When the user logs in with the configured credentials
    Then the login API confirms success
      * the user is authenticated
